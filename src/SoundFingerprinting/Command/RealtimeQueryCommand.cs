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
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Realtime command used to query the underlying data storage in realtime.
    /// </summary>
    public sealed class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration, IRealtimeQueryCommand
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly Queue<Hashes> downtimeHashes;
        
        private const int MinSamplesForOneFingerprint = 10240;

        private IAsyncEnumerable<AudioSamples> realtimeCollection;
        private RealtimeQueryConfiguration configuration;
        private IModelService modelService;
        private IAudioService audioService;
        private Func<Hashes, Hashes> hashesInterceptor = _ => _;

        internal RealtimeQueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            downtimeHashes = new Queue<Hashes>();
            
            configuration = new DefaultRealtimeQueryConfiguration(
                e => { /* do nothing */ }, 
                e => { /* do nothing */ }, (e, _) => throw e, () => {/* do nothing */ });
            realtimeCollection = new BlockingRealtimeCollection<AudioSamples>(new BlockingCollection<AudioSamples>());
            modelService = new InMemoryModelService();
            audioService = new SoundFingerprintingAudioService();
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AudioSamples})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> source)
        {
            realtimeCollection = source;
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{string})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<string> files)
        {
            realtimeCollection = ReadHashesAsync(files);
            return this;
        }

        /// <inheritdoc cref="IWithRealtimeQueryConfiguration.WithRealtimeQueryConfig(RealtimeQueryConfiguration)"/>
        public IInterceptRealtimeHashes WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration)
        {
            configuration = realtimeQueryConfiguration;
            return this;
        }

        /// <inheritdoc cref="IWithRealtimeQueryConfiguration.WithRealtimeQueryConfig(Func{RealtimeQueryConfiguration,RealtimeQueryConfiguration})"/>
        public IInterceptRealtimeHashes WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor)
        {
            configuration = amendQueryFunctor(configuration);
            return this;
        }

        /// <inheritdoc cref="IRealtimeQueryCommand.Query"/>
        public async Task<double> Query(CancellationToken cancellationToken)
        {
            return await QueryAndHash(queryFingerprintService, cancellationToken);
        }

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices"/>
        public IRealtimeQueryCommand UsingServices(IModelService service)
        {
            modelService = service;
            audioService = new SoundFingerprintingAudioService();
            return this;
        }

        /// <inheritdoc cref="IInterceptRealtimeHashes.Intercept"/>
        public IUsingRealtimeQueryServices Intercept(Func<Hashes, Hashes> hashesInterceptor)
        {
            this.hashesInterceptor = hashesInterceptor;
            return this;
        }
        
        private async IAsyncEnumerable<AudioSamples> ReadHashesAsync(IAsyncEnumerable<string> files)
        {
            await foreach (var file in files)
            {
                yield return audioService.ReadMonoSamplesFromFile(file, configuration.QueryConfiguration.FingerprintConfiguration.SampleRate);
            }
        }

        private async Task<double> QueryAndHash(IQueryFingerprintService service, CancellationToken cancellationToken)
        {
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(configuration.Stride, MinSamplesForOneFingerprint);
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(configuration.ResultEntryFilter, 
                configuration.OngoingResultEntryFilter,
                configuration.OngoingSuccessCallback,
                configuration.QueryConfiguration.PermittedGap);

            double queryLength = 0d;
            await foreach (var audioSamples in realtimeCollection.WithCancellation(cancellationToken))
            {
                queryLength += audioSamples.Duration;

                var prefixed = realtimeSamplesAggregator.Aggregate(audioSamples);
                var hashes = await CreateQueryFingerprints(fingerprintCommandBuilder, prefixed);
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
            return await commandBuilder
                .BuildFingerprintCommand()
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