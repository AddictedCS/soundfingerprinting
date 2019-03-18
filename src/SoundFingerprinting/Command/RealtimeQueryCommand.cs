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
        private const int MillisecondsDelay = (int)((double) MinSamplesForOneFingerprint / SupportedFrequency * 1000);

        private IRealtimeCollection realtimeCollection;
        private readonly Queue<TimedHashes> downtimeHashes;
        private RealtimeQueryConfiguration configuration;
        private IModelService modelService;
        private IAudioService audioService;

        public RealtimeQueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            downtimeHashes = new Queue<TimedHashes>();
            
            configuration = new DefaultRealtimeQueryConfiguration(
                e => throw new Exception("Register a success callback for your realtime query."), 
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

        public async Task<double> Hash(CancellationToken cancellationToken)
        {
            return await QueryAndHash(cancellationToken, new DummyQueryFingerprintService());
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
            while (!realtimeCollection.IsAddingCompleted && !cancellationToken.IsCancellationRequested)
            {
                AudioSamples audioSamples;
                try
                {
                    if (!realtimeCollection.TryTake(out audioSamples, MillisecondsDelay, cancellationToken))
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
                
                // TODO fix audioSamples.RelativeTo to prefixed.RelativeTo
                if (!TryQuery(service, hashes, audioSamples.RelativeTo, out var queryResults))
                {
                    continue;
                }

                foreach (var queryResult in queryResults)
                {
                    // TODO fix audioSamples.Duration to prefixed.Duration
                    var aggregatedResult = resultsAggregator.Consume(queryResult.ResultEntries, audioSamples.Duration);
                    InvokeSuccessHandler(aggregatedResult);
                    InvokeDidNotPassFilterHandler(aggregatedResult);
                }
                
            }

            return queryLength;
        }

        private bool TryQuery(IQueryFingerprintService service, List<HashedFingerprint> hashes, DateTime relativeTo, out IEnumerable<QueryResult> results)
        {
            try
            {
                configuration.QueryConfiguration.RelativeTo = relativeTo;
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
                var timedHashes = StoreDowntimeEntries(hashes, relativeTo);
                InvokeExceptionCallback(e, timedHashes);
                results = Enumerable.Empty<QueryResult>();
                return false;
            }
        }

        private void InvokeExceptionCallback(Exception e, TimedHashes timedHashes)
        {
            configuration?.ErrorCallback(e, timedHashes);
        }
        
        private void InvokeHashedFingerprintsCallback(List<HashedFingerprint> hashes)
        {
            configuration?.QueryFingerprintsCallback(hashes);
        }

        private async Task<List<HashedFingerprint>> CreateQueryFingerprints(IFingerprintCommandBuilder commandBuilder, AudioSamples prefixed)
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

        private TimedHashes StoreDowntimeEntries(List<HashedFingerprint> hashes, DateTime relativeTo)
        {
            double length = downtimeHashes.Sum(hash => hash.TotalSeconds);
            var timedHashes = new TimedHashes(hashes, relativeTo);
            if (length <= configuration.DowntimeCapturePeriod)
            {
                downtimeHashes.Enqueue(timedHashes);
                return TimedHashes.Empty;
            }
            
            return timedHashes;
        }

        private IEnumerable<QueryResult> ConsumeDowntimeHashes(IQueryFingerprintService service)
        {
            while (downtimeHashes.Any())
            {
                var timedHashes = downtimeHashes.Dequeue();
                configuration.QueryConfiguration.RelativeTo = timedHashes.StartsAt;
                yield return service.Query(timedHashes.HashedFingerprints, configuration.QueryConfiguration, modelService);
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
                
                configuration.QueryConfiguration.RelativeTo = downtimeHash.StartsAt;
                yield return service.Query(downtimeHash.HashedFingerprints, configuration.QueryConfiguration, modelService);
            }
        }
    }
}