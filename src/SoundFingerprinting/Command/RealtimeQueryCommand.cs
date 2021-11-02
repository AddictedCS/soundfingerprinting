namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    public sealed class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly Queue<Hashes> downtimeHashes;

        private IAsyncEnumerable<AudioSamples> realtimeCollection;
        private RealtimeQueryConfiguration configuration;
        private IModelService modelService;
        private IAudioService audioService;
        private Func<Hashes, Hashes> hashesInterceptor = _ => _;

        private bool errored = false;
        private double queryLength = 0;

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

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IModelService)"/>
        public IRealtimeQueryCommand UsingServices(IModelService service)
        {
            modelService = service;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IModelService,IAudioService)"/>
        public IRealtimeQueryCommand UsingServices(IModelService modelService, IAudioService audioService)
        {
            this.modelService = modelService;
            this.audioService = audioService;
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
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(configuration.Stride);
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator<ResultEntry>(configuration.ResultEntryFilter, 
                configuration.OngoingResultEntryFilter,
                configuration.OngoingSuccessCallback,
                new ResultEntryCompletionStrategy( configuration.QueryConfiguration.PermittedGap),
                new ResultEntryConcatenator(), _ => _.Track.Id);

            while (true)
            {
                try
                {
                    return await QueryRealtimeSource(service, realtimeSamplesAggregator, resultsAggregator, cancellationToken);
                }
                catch (Exception e) when (e is OperationCanceledException || e is ObjectDisposedException)
                {
                    return queryLength;
                }
                catch (Exception e)
                {
                    errored = true;
                    configuration.ErrorCallback(e, null);
                    configuration.ErrorBackoffPolicy.Failure();
                    try
                    {
                        await Task.Delay(configuration.ErrorBackoffPolicy.RemainingDelay, cancellationToken);
                    }
                    catch (Exception delayException) when (delayException is OperationCanceledException || delayException is ObjectDisposedException)
                    {
                        return queryLength;
                    }
                }
            }
        }

        private async Task<double> QueryRealtimeSource(IQueryFingerprintService service, 
            IRealtimeAudioSamplesAggregator realtimeSamplesAggregator,
            IRealtimeAggregator<ResultEntry> resultsAggregator, 
            CancellationToken cancellationToken)
        {
            await foreach (var audioSamples in realtimeCollection.WithCancellation(cancellationToken))
            {
                queryLength += audioSamples.Duration;
                var prefixed = realtimeSamplesAggregator.Aggregate(audioSamples);
                if (prefixed == null)
                {
                    continue;
                }

                var fingerprintingStopwatch = Stopwatch.StartNew();
                double timeOffset = audioSamples.Duration - prefixed.Duration;
                var hashes = (await CreateQueryFingerprints(fingerprintCommandBuilder, prefixed)).WithTimeOffset(timeOffset);
                hashes = hashesInterceptor(hashes);
                var fingerprintingDuration = fingerprintingStopwatch.ElapsedMilliseconds;
                if (!TryQuery(service, hashes, out var queryResults))
                {
                    continue;
                }

                foreach (var queryResult in queryResults)
                {
                    var aggregatedResult = resultsAggregator.Consume(queryResult.ResultEntries, queryResult.QueryHashes.DurationInSeconds, queryResult.QueryHashes.TimeOffset);
                    var queryCommandStats = queryResult.CommandStats.WithFingerprintingDurationMilliseconds(fingerprintingDuration);
                    InvokeSuccessHandler(aggregatedResult.SuccessEntries, queryResult.QueryHashes, queryCommandStats);
                    InvokeDidNotPassFilterHandler(aggregatedResult.DidNotPassThresholdEntries, queryResult.QueryHashes, queryCommandStats);
                }
            }

            var purged = resultsAggregator.Purge();
            InvokeSuccessHandler(purged.SuccessEntries, Hashes.GetEmpty(MediaType.Audio), new QueryCommandStats(0, 0, 0, 0));
            InvokeDidNotPassFilterHandler(purged.DidNotPassThresholdEntries, Hashes.GetEmpty(MediaType.Audio), new QueryCommandStats(0, 0, 0, 0));
            return queryLength;
        }

        private bool TryQuery(IQueryFingerprintService service, Hashes hashes, out IEnumerable<QueryResult> results)
        {
            try
            {
                var result = service.Query(hashes, configuration.QueryConfiguration, modelService);
                if (errored)
                {
                    errored = false;
                    configuration.RestoredAfterErrorCallback();
                    configuration.ErrorBackoffPolicy.Success();
                }
                
                if (!downtimeHashes.Any())
                {
                    results = new[] {result}.AsEnumerable();
                    return true;
                }

                results = ConsumeDowntimeHashes(service)
                    .Concat(ConsumeExternalDowntimeHashes(service))
                    .Concat(new[] {result});

                return true;
            }
            catch (Exception e)
            {
                errored = true;
                var timedHashes = StoreDowntimeEntries(hashes);
                configuration.ErrorCallback(e, timedHashes);
                configuration.ErrorBackoffPolicy.Failure();
                results = Enumerable.Empty<QueryResult>();
                return false;
            }
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

        private void InvokeDidNotPassFilterHandler(IReadOnlyCollection<ResultEntry> resultEntries, Hashes hashes, QueryCommandStats queryCommandStats)
        {
            if (resultEntries.Any())
            {
                var result = new QueryResult(resultEntries, hashes, queryCommandStats);
                configuration?.DidNotPassFilterCallback(result);
            }
        }

        private void InvokeSuccessHandler(IReadOnlyCollection<ResultEntry> resultEntries, Hashes hashes, QueryCommandStats queryCommandStats)
        {
            if (resultEntries.Any())
            {
                var entries = resultEntries.OrderByDescending(_ => _.Confidence).ThenBy(e => e.Track.Id).ToList();
                var result = new QueryResult(entries, hashes, queryCommandStats);
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
            foreach (var downtimeHash in configuration.OfflineStorage)
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