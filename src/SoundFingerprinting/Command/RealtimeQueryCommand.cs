namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Content;
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
        
        private IAsyncEnumerable<AVHashes> realtimeCollection;
        private RealtimeQueryConfiguration configuration;
        private IModelService modelService;
        private IAudioService audioService;
        private Func<AVHashes, AVHashes> hashesInterceptor = _ => _;
        
        private bool errored = false;
        private double queryLength = 0;

        internal RealtimeQueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            
            configuration = new DefaultRealtimeQueryConfiguration(
                e => { /* do nothing */ }, 
                e => { /* do nothing */ }, (e, _) => throw e, () => {/* do nothing */ });
            realtimeCollection = new BlockingRealtimeCollection<AVHashes>(new BlockingCollection<AVHashes>());
            modelService = new InMemoryModelService();
            audioService = new SoundFingerprintingAudioService();
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AudioSamples})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> source)
        {
            realtimeCollection = ConvertToAvHashes(ConvertToAvTrack(source));
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{string})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<string> files)
        {
            realtimeCollection = ConvertToAvHashes(ConvertToAvTrack(ReadHashesAsync(files)));
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AVTrack})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AVTrack> tracks)
        {
            realtimeCollection = ConvertToAvHashes(tracks);
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AVHashes})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AVHashes> avHashes)
        {
            realtimeCollection = avHashes;
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
            try
            {
                await Task.Yield();
                return await QueryRealtimeSource(queryFingerprintService, cancellationToken);
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
            {
                return queryLength;
            }
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
        public IUsingRealtimeQueryServices Intercept(Func<AVHashes, AVHashes> hashesInterceptor)
        {
            this.hashesInterceptor = hashesInterceptor;
            return this;
        }
        
        private async IAsyncEnumerable<AudioSamples> ReadHashesAsync(IAsyncEnumerable<string> files)
        {
            await foreach (var file in files)
            {
                yield return audioService.ReadMonoSamplesFromFile(file, configuration.QueryConfiguration.Audio.FingerprintConfiguration.SampleRate);
            }
        }
        
        private static async IAsyncEnumerable<AVTrack> ConvertToAvTrack(IAsyncEnumerable<AudioSamples> source)
        {
            await foreach (var samples in source)
            {
                yield return new AVTrack(new AudioTrack(samples, samples.Duration), null);
            }
        }

        private async IAsyncEnumerable<AVHashes> ConvertToAvHashes(IAsyncEnumerable<AVTrack> source)
        {
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(configuration.QueryConfiguration.Audio.FingerprintConfiguration.SpectrogramConfig.MinimumSamplesPerFingerprint, configuration.QueryConfiguration.Audio.Stride);
            await foreach (var (audioTrack, videoTrack) in source)
            {
                var prefixed = audioTrack != null ?  realtimeSamplesAggregator.Aggregate(audioTrack.Samples) : null;
                var fingerprintingStopwatch = Stopwatch.StartNew();
                double audioTimeOffset = (audioTrack?.Samples.Duration ?? 0) - (prefixed?.Duration ?? 0);
                var audioHashes = prefixed != null ? (await CreateQueryFingerprints(fingerprintCommandBuilder, prefixed)) : null;
                audioHashes = audioHashes?.WithTimeOffset(audioTimeOffset);
                long audioFingerprintingDuration = fingerprintingStopwatch.ElapsedMilliseconds;
                fingerprintingStopwatch.Restart();
                var videoHashes = videoTrack?.Frames != null ? await CreateQueryFingerprints(fingerprintCommandBuilder, videoTrack.Frames) : null;
                long videoFingerprintingDuration = fingerprintingStopwatch.ElapsedMilliseconds;
                if (audioHashes == null && videoHashes == null)
                {
                    continue;
                }
                
                var avHashes = hashesInterceptor(new AVHashes(audioHashes, videoHashes, new AVFingerprintingStats(audioFingerprintingDuration, videoFingerprintingDuration)));
                fingerprintingStopwatch.Stop();
                yield return avHashes;
            }
        }

        /// <summary>
        ///  Query realtime source and associated <see cref="IOfflineStorage"/>.
        /// </summary>
        /// <param name="service">An instance of <see cref="IQueryFingerprintService"/> with configured <see cref="IModelService"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Query length.</returns>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        private async Task<double> QueryRealtimeSource(IQueryFingerprintService service, CancellationToken cancellationToken)
        {
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(configuration.ResultEntryFilter, 
                configuration.OngoingResultEntryFilter,
                configuration.OngoingSuccessCallback,
                new AVResultEntryCompletionStrategy(configuration.QueryConfiguration),
                new ResultEntryConcatenator(),
                new StatefulQueryHashesConcatenator());

            try
            {
                // lets check the offline storage immediately at startup
                await foreach (var queryResult in QueryFromOfflineStorage(service, cancellationToken))
                {
                    ConsumeQueryResult(queryResult, resultsAggregator);
                }
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
            {
                // if operation has been cancelled re-throw to the calling thread.
                throw;
            }
            catch (Exception e)
            {
                // if an offline storage exception occurs, let's continue consuming realtime data and storing it to the offline storage
                await HandleQueryFailure(null, cancellationToken, e);
            }

            while (true)
            {
                try
                {
                    await QueryFromRealtimeAndOffline(service, resultsAggregator, cancellationToken);
                    var purged = resultsAggregator.Purge();
                    InvokeSuccessHandler(purged.SuccessEntries);
                    InvokeDidNotPassFilterHandler(purged.DidNotPassThresholdEntries);
                    return queryLength;
                }
                catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    await HandleQueryFailure(null, cancellationToken, e);
                }
            }
        }

        /// <summary>
        ///  Query from realtime and offline sources.
        /// </summary>
        /// <param name="service">An instance of <see cref="IQueryFingerprintService"/> with configured <see cref="IModelService"/>.</param>
        /// <param name="resultsAggregator">Results aggregator to use for query results aggregation.</param>
        /// <param name="cancellationToken">Cancellation token to cancel querying.</param>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        /// <exception cref="Exception">Any other exception that can occur during fingerprinting creation, and not fingerprinting querying.</exception>
        private async Task QueryFromRealtimeAndOffline(IQueryFingerprintService service, IRealtimeAggregator resultsAggregator, CancellationToken cancellationToken)
        {
            await foreach (var avHashes in realtimeCollection.WithCancellation(cancellationToken))
            {
                await TryQuery(service, avHashes, resultsAggregator, cancellationToken);
            }
        }

        private void ConsumeQueryResult(AVQueryResult queryResult, IRealtimeAggregator resultsAggregator)
        {
            var aggregatedResult = resultsAggregator.Consume(queryResult);
            InvokeSuccessHandler(aggregatedResult.SuccessEntries);
            InvokeDidNotPassFilterHandler(aggregatedResult.DidNotPassThresholdEntries);
        }
        
        private async Task HandleQueryFailure(AVHashes? hashes, CancellationToken cancellationToken, Exception e)
        {
            errored = true;
            configuration.ErrorCallback(e, hashes);
            configuration.ErrorBackoffPolicy.Failure();
            configuration.OfflineStorage.Add(hashes);
            await Task.Delay(configuration.ErrorBackoffPolicy.RemainingDelay, cancellationToken);
        }

        /// <summary>
        ///  Queries local or remote <see cref="IModelService"/> with <see cref="AVHashes"/> gathered from offline storage.
        /// </summary>
        /// <param name="service">An instance of <see cref="IQueryFingerprintService"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Async enumerable with <see cref="AVQueryResult"/>.</returns>
        /// <exception cref="IOException">Input/Output exception when querying the model service.</exception>> 
        private async IAsyncEnumerable<AVQueryResult> QueryFromOfflineStorage(IQueryFingerprintService service, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var offlineStorage = configuration.OfflineStorage;
            while (offlineStorage.Any())
            {
                var offlineHashes = offlineStorage.First();
                yield return GetAVQueryResult(service, offlineHashes);
                offlineStorage.Remove(offlineHashes);
                await Task.Delay(configuration.DelayStrategy.Delay, cancellationToken);
            }
        }

        /// <summary>
        ///  Tries querying associated <see cref="IModelService"/>.
        /// </summary>
        /// <param name="service">An instance of <see cref="IQueryFingerprintService"/>.</param>
        /// <param name="hashes">An instance of <see cref="AVHashes"/>.</param>
        /// <param name="realtimeAggregator">Realtime aggregator.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if query was successful, otherwise false</returns>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        private async Task<bool> TryQuery(IQueryFingerprintService service, AVHashes hashes, IRealtimeAggregator realtimeAggregator, CancellationToken cancellationToken)
        {
            try
            {
                var avQueryResult = GetAVQueryResult(service, hashes).WithFingerprintingDurationMilliseconds(hashes.Stats.Audio, hashes.Stats.Video);
                if (errored)
                {
                    errored = false;
                    configuration.RestoredAfterErrorCallback();
                    configuration.ErrorBackoffPolicy.Success();
                }

                await foreach (var offlineResult in QueryFromOfflineStorage(service, cancellationToken))
                {
                    ConsumeQueryResult(offlineResult, realtimeAggregator);
                }
                
                ConsumeQueryResult(avQueryResult, realtimeAggregator);
                return true;
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
            {
                throw;
            }
            catch (Exception e)
            {
                await HandleQueryFailure(hashes, cancellationToken, e);
                return false;
            }
        }

        private async Task<Hashes> CreateQueryFingerprints(IFingerprintCommandBuilder commandBuilder, AudioSamples prefixed)
        {
            return await commandBuilder
                .BuildFingerprintCommand()
                .From(prefixed)
                .WithFingerprintConfig(configuration.QueryConfiguration.Audio.FingerprintConfiguration)
                .Hash();
        }

        private async Task<Hashes> CreateQueryFingerprints(IFingerprintCommandBuilder commandBuilder, Frames frames)
        {
            return await commandBuilder
                .BuildFingerprintCommand()
                .From(frames)
                .WithFingerprintConfig(configuration.QueryConfiguration.Video.FingerprintConfiguration)
                .Hash();
        }

        /// <summary>
        ///  Gets an instance of <see cref="AVQueryResult"/> with query results from both Audio and Video queries.
        /// </summary>
        /// <param name="service">An instance of <see cref="IQueryFingerprintService"/> to use to query.</param>
        /// <param name="hashes">An instance of <see cref="AVHashes"/>.</param>
        /// <returns>An instance of <see cref="AVQueryResult"/>.</returns>
        /// <exception cref="IOException">Input/Output exception when querying the model service.</exception>> 
        private AVQueryResult GetAVQueryResult(IQueryFingerprintService service, AVHashes hashes)
        {
            var audio = hashes.Audio != null ? service.Query(hashes.Audio, configuration.QueryConfiguration.Audio, modelService) : null;
            var video = hashes.Video != null ? service.Query(hashes.Video, configuration.QueryConfiguration.Video, modelService) : null;
            queryLength += ((hashes.Audio?.DurationInSeconds + hashes.Audio?.TimeOffset) ?? hashes.Video?.DurationInSeconds) ?? 0;
            var avResultEntries = AVQueryResult.JoinResultEntries(audio, video);
            return new AVQueryResult(avResultEntries, hashes, new AVQueryCommandStats(audio?.CommandStats, video?.CommandStats));
        }
        
        private void InvokeDidNotPassFilterHandler(IEnumerable<AVQueryResult> queryResults)
        {
            foreach (var queryResult in queryResults)
            {
                configuration?.DidNotPassFilterCallback(queryResult);
            }
        }

        private void InvokeSuccessHandler(IEnumerable<AVQueryResult> queryResults)
        {
            foreach (var queryResult in queryResults)
            {
                configuration?.SuccessCallback(queryResult);
            }
        }
    }
}