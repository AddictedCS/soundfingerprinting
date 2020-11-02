namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration, IUsingRealtimeQueryServices, IRealtimeQueryCommand
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private const int MinSamplesForOneFingerprint = 10240;
        private const int SupportedFrequency = 5512;

        private IRealtimeCollection realtimeCollection;
        private readonly Queue<Hashes> downtimeHashes;
        private RealtimeQueryConfiguration configuration;
        private IModelService modelService;
        private IAudioService audioService;

        public RealtimeQueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            downtimeHashes = new Queue<Hashes>();
            
            configuration = new DefaultRealtimeQueryConfiguration(
                e => { /* do nothing */ }, 
                e => { /* do nothing */ }, fingerprints => { /* do nothing */ }, (e, _) => throw e, () => {/* do nothing */ });
        }

        public IWithRealtimeQueryConfiguration From(BlockingCollection<AudioSamples> audioSamples)
        {
            realtimeCollection = new RealtimeCollection(audioSamples);
            return this;
        }

        public IWithRealtimeQueryConfiguration From(IRealtimeCollection collection)
        {
            realtimeCollection = collection;
            return this;
        }

        public IUsingRealtimeQueryServices WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration)
        {
            configuration = realtimeQueryConfiguration;
            return this;
        }

        public IUsingRealtimeQueryServices WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor)
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
        
        private async Task<double> QueryAndHash(CancellationToken cancellationToken, IQueryFingerprintService service)
        {
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(configuration.Stride, MinSamplesForOneFingerprint);
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(configuration.ResultEntryFilter, configuration.PermittedGap);

            double queryLength = 0d;
            while (!realtimeCollection.IsFinished && !cancellationToken.IsCancellationRequested)
            {
                AudioSamples audioSamples;
                try
                {
                    if (!realtimeCollection.TryTake(out audioSamples, configuration.MillisecondsDelay, cancellationToken))
                    {
                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    return queryLength;
                }

                if (audioSamples.SampleRate != SupportedFrequency)
                {
                    throw new ArgumentException($"{nameof(audioSamples)} should be provided down sampled to {SupportedFrequency}Hz");
                }

                queryLength += audioSamples.Duration;

                var prefixed = realtimeSamplesAggregator.Aggregate(audioSamples);
                var hashes = await CreateQueryFingerprints(fingerprintCommandBuilder, prefixed);
                InvokeHashedFingerprintsCallback(hashes);
                
                if (!TryQuery(service, hashes, out var queryResults))
                {
                    continue;
                }

                foreach (var queryResult in queryResults)
                {
                    var aggregatedResult = resultsAggregator.Consume(queryResult.ResultEntries, prefixed.Duration);
                    InvokeSuccessHandler(aggregatedResult);
                    InvokeDidNotPassFilterHandler(aggregatedResult);
                }
            }

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
        
        private void InvokeHashedFingerprintsCallback(Hashes hashes)
        {
            configuration?.QueryFingerprintsCallback(hashes);
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
            while (downtimeHashes.Any())
            {
                var timedHashes = downtimeHashes.Dequeue();
                yield return service.Query(timedHashes, configuration.QueryConfiguration, modelService);
            }
        }

        private IEnumerable<QueryResult> ConsumeExternalDowntimeHashes(IQueryFingerprintService service)
        {
            foreach (var downtimeHash in configuration.DowntimeHashes)
            {
                if (downtimeHash.IsEmpty)
                {
                    continue;
                }
                
                yield return service.Query(downtimeHash, configuration.QueryConfiguration, modelService);
            }
        }
    }
}