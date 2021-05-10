namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration, IRealtimeQueryCommand
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private const int MinSamplesForOneFingerprint = 10240;
        private const int SupportedFrequency = 5512;

        private IAsyncEnumerable<AudioSamples> realtimeCollection;
        private readonly Queue<Hashes> downtimeHashes;
        private RealtimeQueryConfiguration configuration;
        private IModelService modelService;
        private IAudioService audioService;
        private string streamId = string.Empty;
        private Func<Hashes, Hashes> hashesInterceptor = _ => _;

        public RealtimeQueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            downtimeHashes = new Queue<Hashes>();
            
            configuration = new DefaultRealtimeQueryConfiguration(
                e => { /* do nothing */ }, 
                e => { /* do nothing */ }, (e, _) => throw e, () => {/* do nothing */ });
        }

        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> realtimeCollection)
        {
            return From(realtimeCollection, string.Empty);
        }
        
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> realtimeCollection, string streamId)
        {
            this.realtimeCollection = realtimeCollection;
            this.streamId = streamId;
            return this;
        }

        public IInterceptRealtimeHashes WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration)
        {
            configuration = realtimeQueryConfiguration;
            return this;
        }

        public IInterceptRealtimeHashes WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor)
        {
            configuration = amendQueryFunctor(configuration);
            return this;
        }

        public async Task<double> Query(CancellationToken cancellationToken)
        {
            return await QueryAndHash(cancellationToken, queryFingerprintService);
        }

        public IRealtimeQueryCommand UsingServices(IModelService service)
        {
            modelService = service;
            audioService = new SoundFingerprintingAudioService();
            return this;
        }

        public IUsingRealtimeQueryServices Intercept(Func<Hashes, Hashes> hashesInterceptor)
        {
            this.hashesInterceptor = hashesInterceptor;
            return this;
        }

        private async Task<double> QueryAndHash(CancellationToken cancellationToken, IQueryFingerprintService service)
        {
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(configuration.Stride, MinSamplesForOneFingerprint);
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(configuration.ResultEntryFilter, configuration.QueryConfiguration);

            double queryLength = 0d;
            await foreach (var audioSamples in realtimeCollection.WithCancellation(cancellationToken))
            {
                if (audioSamples.SampleRate != SupportedFrequency)
                {
                    throw new ArgumentException($"{nameof(audioSamples)} should be provided down sampled to {SupportedFrequency}Hz");
                }

                queryLength += audioSamples.Duration;

                var prefixed = realtimeSamplesAggregator.Aggregate(audioSamples);
                var hashes = (await CreateQueryFingerprints(fingerprintCommandBuilder, prefixed)).WithStreamId(streamId);
                hashes = hashesInterceptor(hashes).WithTimeOffset(audioSamples.Duration - hashes.DurationInSeconds);
                
                if (!TryQuery(service, hashes, out var queryResults))
                {
                    continue;
                }

                foreach (var queryResult in queryResults)
                {
                    var aggregatedResult = resultsAggregator.Consume(queryResult.ResultEntries, queryResult.QueryHashes.DurationInSeconds, queryResult.QueryHashes.TimeOffset);
                    InvokeSuccessHandler(aggregatedResult);
                    InvokeDidNotPassFilterHandler(aggregatedResult);
                }
            }

            var purged = resultsAggregator.Purge();
            InvokeSuccessHandler(purged);
            InvokeDidNotPassFilterHandler(purged); 
            return queryLength;
        }

        private bool TryQuery(IQueryFingerprintService service, Hashes hashes, out IEnumerable<QueryResult> results)
        {
            try
            {
                var result = service.Query(hashes, configuration.QueryConfiguration, modelService);
                if (!downtimeHashes.Any())
                {
                    results = new[] {result}.AsEnumerable();
                    return true;
                }

                results = ConsumeDowntimeHashes(service)
                    .Concat(ConsumeExternalDowntimeHashes(service))
                    .Concat(new[] {result});
                configuration?.RestoredAfterErrorCallback.Invoke();
                return true;

            }
            catch (Exception e)
            {
                var timedHashes = StoreDowntimeEntries(hashes);
                InvokeExceptionCallback(e, timedHashes);
                results = Enumerable.Empty<QueryResult>();
                return false;
            }
        }

        private void InvokeExceptionCallback(Exception e, Hashes hashes)
        {
            configuration?.ErrorCallback(e, hashes);
        }
        
        private async Task<Hashes> CreateQueryFingerprints(IFingerprintCommandBuilder commandBuilder, AudioSamples prefixed)
        {
            return await commandBuilder.BuildFingerprintCommand()
                .From(prefixed)
                .WithFingerprintConfig(configuration.QueryConfiguration.FingerprintConfiguration)
                .UsingServices(audioService)
                .Hash();
        }

        private void InvokeDidNotPassFilterHandler(RealtimeQueryResult realtimeQueryResult)
        {
            foreach (var result in realtimeQueryResult.DidNotPassThresholdEntries)
            {
                configuration?.DidNotPassFilterCallback(result);
            }
        }

        private void InvokeSuccessHandler(RealtimeQueryResult realtimeQueryResult)
        {
            foreach (var result in realtimeQueryResult.SuccessEntries)
            {
                configuration?.SuccessCallback(result);
            }
        }

        private Hashes StoreDowntimeEntries(Hashes hashes)
        {
            double length = downtimeHashes.Sum(hash => hash.DurationInSeconds);
            if (length <= configuration.DowntimeCapturePeriod)
            {
                downtimeHashes.Enqueue(hashes);
                return Hashes.GetEmpty(hashes.MediaType);
            }
            
            return hashes;
        }

        private IEnumerable<QueryResult> ConsumeDowntimeHashes(IQueryFingerprintService service)
        {
            var list = new List<QueryResult>();
            while (downtimeHashes.Any())
            {
                var timedHashes = downtimeHashes.Dequeue();
                var results = service.Query(timedHashes, configuration.QueryConfiguration, modelService);
                list.Add(results);
            }

            return list;
        }

        private IEnumerable<QueryResult> ConsumeExternalDowntimeHashes(IQueryFingerprintService service)
        {
            var list = new List<QueryResult>();
            foreach (var downtimeHash in configuration.DowntimeHashes)
            {
                if (downtimeHash.IsEmpty)
                {
                    continue;
                }
                
                var results = service.Query(downtimeHash, configuration.QueryConfiguration, modelService);
                list.Add(results);
            }

            return list;
        }
    }
}