namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Realtime command used to query the underlying data storage in realtime.
    /// </summary>
    public sealed class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration
    {
        private readonly ILogger<RealtimeQueryCommand> logger;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryCommandBuilder queryCommandBuilder;
        private readonly ILoggerFactory loggerFactory;

        private Func<CancellationToken, IAsyncEnumerable<AVHashes>> realtimeCollection;
        private RealtimeQueryConfiguration configuration;
        private IRealtimeMediaService? realtimeMediaService;
        private IModelService? modelService;
        private IMediaService? mediaService;
        private IVideoService? videoService;
        private IAudioService audioService;
        private Func<AVTrack, AVTrack> avTrackInterceptor = _ => _;
        private Func<AVHashes, AVHashes> hashesInterceptor = _ => _;
        private Func<AVQueryResult, AVQueryResult> queryResultInterceptor = _ => _;
        
        private bool errored = false;
        private double queryLength = 0;

        internal RealtimeQueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryCommandBuilder queryCommandBuilder, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<RealtimeQueryCommand>();
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryCommandBuilder = queryCommandBuilder;
            this.loggerFactory = loggerFactory;

            configuration = new DefaultRealtimeQueryConfiguration(
                e => { /* do nothing */ }, 
                e => { /* do nothing */ }, (e, _) => throw e, () => {/* do nothing */ });
            realtimeCollection = _ => new BlockingRealtimeCollection<AVHashes>(new BlockingCollection<AVHashes>());
            audioService = new SoundFingerprintingAudioService();
        }
        
        /// <inheritdoc cref="IRealtimeSource.From(string,double,MediaType)"/>
        public IWithRealtimeQueryConfiguration From(string url, double chunkLength, MediaType mediaType = MediaType.Audio)
        {
            realtimeCollection = cancellationToken =>
            {
                if (realtimeMediaService == null)
                {
                    throw new ArgumentException("Set an instance of IRealtimeMediaService in UsingServices method to be able to generate fingerprints directly from broadcast URL");
                }
                
                var avTrackReadConfiguration = configuration.QueryConfiguration.FingerprintConfiguration.GetTrackReadConfiguration();
                return ConvertToAvHashes(realtimeMediaService.ReadAVTrackFromRealtimeSource(url, chunkLength, avTrackReadConfiguration, mediaType, cancellationToken));
            };
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AudioSamples})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> source)
        {
            realtimeCollection = _ => ConvertToAvHashes(ConvertToAvTrack(source));
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{string},MediaType)"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<string> files, MediaType mediaType = MediaType.Audio)
        {
            realtimeCollection = cancellationToken => ConvertToAvHashes(ReadHashesAsync(files, cancellationToken, mediaType));
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{StreamingFile})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<StreamingFile> files)
        {
            realtimeCollection = cancellationToken => ReadHashesAsync(files, cancellationToken);
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AVTrack})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AVTrack> tracks)
        {
            realtimeCollection = _ => ConvertToAvHashes(tracks);
            return this;
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AVHashes})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AVHashes> avHashes)
        {
            realtimeCollection = _ => avHashes;
            return this;
        }

        /// <inheritdoc cref="IWithRealtimeQueryConfiguration.WithRealtimeQueryConfig(RealtimeQueryConfiguration)"/>
        public IInterceptRealtimeSource WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration)
        {
            configuration = realtimeQueryConfiguration;
            return this;
        }

        /// <inheritdoc cref="IWithRealtimeQueryConfiguration.WithRealtimeQueryConfig(Func{RealtimeQueryConfiguration,RealtimeQueryConfiguration})"/>
        public IInterceptRealtimeSource WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor)
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
                return await QueryRealtimeSource(cancellationToken);
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

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IModelService,IMediaService)"/>
        public IRealtimeQueryCommand UsingServices(IModelService modelService, IMediaService mediaService)
        {
            this.modelService = modelService;
            this.mediaService = mediaService;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IModelService,IVideoService)"/>
        public IRealtimeQueryCommand UsingServices(IModelService modelService, IVideoService videoService)
        {
            this.modelService = modelService;
            this.videoService = videoService;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IModelService,IRealtimeMediaService)"/>
        public IRealtimeQueryCommand UsingServices(IModelService modelService, IRealtimeMediaService realtimeMediaService)
        {
            this.modelService = modelService;
            this.realtimeMediaService = realtimeMediaService;
            return this;
        }

        /// <inheritdoc cref="IInterceptRealtimeSource.InterceptAVTrack"/>
        public IInterceptRealtimeSource InterceptAVTrack(Func<AVTrack, AVTrack> avTrackInterceptor)
        {
            this.avTrackInterceptor = avTrackInterceptor;
            return this;
        }

        /// <inheritdoc cref="IInterceptRealtimeSource.InterceptHashes"/>
        public IInterceptRealtimeSource InterceptHashes(Func<AVHashes, AVHashes> hashesInterceptor)
        {
            this.hashesInterceptor = hashesInterceptor;
            return this;
        }

        /// <inheritdoc cref="IInterceptRealtimeSource.InterceptQueryResults"/>
        public IInterceptRealtimeSource InterceptQueryResults(Func<AVQueryResult, AVQueryResult> queryResultInterceptor)
        {
            this.queryResultInterceptor = queryResultInterceptor;
            return this;
        }

        private async IAsyncEnumerable<AVTrack> ReadHashesAsync(IAsyncEnumerable<string> files, [EnumeratorCancellation] CancellationToken cancellationToken, MediaType mediaType = MediaType.Audio)
        {
            await foreach (string file in files.WithCancellation(cancellationToken))
            {
                yield return GetAvTrack(file, mediaType);
            }
        }

        private static async IAsyncEnumerable<AVTrack> ConvertToAvTrack(IAsyncEnumerable<AudioSamples> source)
        {
            await foreach (var samples in source)
            {
                yield return new AVTrack(new AudioTrack(samples), null);
            }
        }

        private async IAsyncEnumerable<AVHashes> ReadHashesAsync(IAsyncEnumerable<StreamingFile> files, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var realtimeSamplesAggregator = CreateRealtimeAudioSamplesAggregator();
            await foreach (var file in files.WithCancellation(cancellationToken))
            {
                logger.LogDebug("Consuming streaming file {File}", file);
                var avTrack = GetAvTrack(file.Path, file.MediaType);
                var hashes = await GetAvHashes(avTrack, realtimeSamplesAggregator);
                if (hashes == null)
                {
                    continue;
                }

                yield return hashes.WithRelativeTo(file.RelativeTo).WithStreamId(file.StreamId);
            }
        }

        private async IAsyncEnumerable<AVHashes> ConvertToAvHashes(IAsyncEnumerable<AVTrack> source)
        {
            var realtimeSamplesAggregator = CreateRealtimeAudioSamplesAggregator();
            await foreach (var track in source)
            {
                var hashes = await GetAvHashes(track, realtimeSamplesAggregator);
                if (hashes == null)
                {
                    continue;
                }

                yield return hashes;
            }
        }

        /// <summary>
        ///  Query realtime source and associated <see cref="IOfflineStorage"/>.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Query length.</returns>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        private async Task<double> QueryRealtimeSource(CancellationToken cancellationToken)
        {
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(configuration.ResultEntryFilter, 
                configuration.OngoingResultEntryFilter,
                configuration.OngoingSuccessCallback,
                new AVResultEntryCompletionStrategy(configuration.QueryConfiguration),
                new ResultEntryConcatenator(loggerFactory, configuration.AutomaticSkipDetection),
                new ResultEntryConcatenator(loggerFactory, configuration.AutomaticSkipDetection),
                configuration.IncludeQueryHashesInResponse ? new StatefulQueryHashesConcatenator() : new NoOpQueryHashesConcatenator(loggerFactory));

            try
            {
                // lets check the offline storage immediately at startup
                await foreach (var queryResult in QueryFromOfflineStorage(cancellationToken))
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
                HandleQueryFailure(null, e);
            }

            while (true)
            {
                try
                {
                    await QueryFromRealtimeAndOffline(resultsAggregator, cancellationToken);
                    logger.LogInformation("Stopped querying realtime source after {QueryLength:00} seconds", queryLength);
                    return queryLength;
                }
                catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // here we catch exceptions that occur while reading from the realtime source
                    HandleQueryFailure(null, e);
                    await Task.Delay(configuration.ErrorBackoffPolicy.RemainingDelay, cancellationToken);
                }
                finally
                {
                    // let's purge stateful results aggregator to safe-guard ourselves from memory issues when certain tracks get stuck in the aggregator
                    var purged = resultsAggregator.Purge();
                    InvokeSuccessHandler(purged.SuccessEntries);
                    InvokeDidNotPassFilterHandler(purged.DidNotPassThresholdEntries); 
                }
            }
        }

        /// <summary>
        ///  Query from realtime and offline sources.
        /// </summary>
        /// <param name="resultsAggregator">Results aggregator to use for query results aggregation.</param>
        /// <param name="cancellationToken">Cancellation token to cancel querying.</param>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        /// <exception cref="Exception">Any other exception that can occur during fingerprinting creation, and not fingerprinting querying.</exception>
        private async Task QueryFromRealtimeAndOffline(IRealtimeAggregator resultsAggregator, CancellationToken cancellationToken)
        {
            await foreach (var avHashes in realtimeCollection(cancellationToken).WithCancellation(cancellationToken))
            {
                var intercepted = hashesInterceptor(avHashes);
                logger.LogDebug("Issuing query with hashes {Hashes}", intercepted);
                await TryQuery(intercepted, resultsAggregator, cancellationToken);
            }
        }

        private void ConsumeQueryResult(AVQueryResult queryResult, IRealtimeAggregator resultsAggregator)
        {
            var aggregatedResult = resultsAggregator.Consume(queryResult);
            InvokeSuccessHandler(aggregatedResult.SuccessEntries);
            InvokeDidNotPassFilterHandler(aggregatedResult.DidNotPassThresholdEntries);
        }
        
        private void HandleQueryFailure(AVHashes? hashes, Exception e)
        {
            errored = true;
            configuration.ErrorCallback(e, hashes);
            configuration.ErrorBackoffPolicy.Failure();
            configuration.OfflineStorage.Add(hashes);
        }

        /// <summary>
        ///  Queries local or remote <see cref="IModelService"/> with <see cref="AVHashes"/> gathered from offline storage.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Async enumerable with <see cref="AVQueryResult"/>.</returns>
        /// <exception cref="IOException">Input/Output exception when querying the model service.</exception>> 
        private async IAsyncEnumerable<AVQueryResult> QueryFromOfflineStorage([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var offlineStorage = configuration.OfflineStorage;
            while (offlineStorage.Any())
            {
                var offlineHashes = offlineStorage.First();
                logger.LogDebug("Read AVHashes from offline storage {Hashes}. Querying storage", offlineHashes);
                yield return await GetAvQueryResult(offlineHashes);
                offlineStorage.Remove(offlineHashes);
                await Task.Delay(configuration.DelayStrategy.Delay, cancellationToken);
            }
        }

        /// <summary>
        ///  Tries querying associated <see cref="IModelService"/>.
        /// </summary>
        /// <param name="hashes">An instance of <see cref="AVHashes"/>.</param>
        /// <param name="realtimeAggregator">Realtime aggregator.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if query was successful, otherwise false.</returns>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        private async Task<bool> TryQuery(AVHashes hashes, IRealtimeAggregator realtimeAggregator, CancellationToken cancellationToken)
        {
            try
            {
                var (audioMilliseconds, videoMilliseconds) = hashes.FingerprintingTime;
                var avQueryResult = (await GetAvQueryResult(hashes)).WithFingerprintingDurationMilliseconds(audioMilliseconds, videoMilliseconds);
                if (errored)
                {
                    logger.LogDebug("Query restored from previous error");
                    errored = false;
                    configuration.RestoredAfterErrorCallback();
                    configuration.ErrorBackoffPolicy.Success();
                }

                await foreach (var offlineResult in QueryFromOfflineStorage(cancellationToken))
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
                // here we catch exceptions that are related to server not reachable
                HandleQueryFailure(hashes, e);
                return false;
            }
        }
        
        private RealtimeAudioSamplesAggregator CreateRealtimeAudioSamplesAggregator()
        {
            return new RealtimeAudioSamplesAggregator(
                configuration.QueryConfiguration.Audio.FingerprintConfiguration.SpectrogramConfig.MinimumSamplesPerFingerprint,
                configuration.QueryConfiguration.Audio.Stride);
        }
        
        private async Task<AVHashes?> GetAvHashes(AVTrack track, IRealtimeAudioSamplesAggregator realtimeSamplesAggregator)
        {
            logger.LogDebug("Retrieved track {Track} from realtime query source", track);
            var (audioTrack, videoTrack) = avTrackInterceptor(track);
            var prefixed = audioTrack != null ? realtimeSamplesAggregator.Aggregate(audioTrack.Samples) : null;
            if (prefixed == null && videoTrack == null)
            {
                // can happen when there is not enough samples to generate a fingerprint
                logger.LogDebug("Both audio and video tracks are null or empty. Waiting until minimum number of samples are aggregated for a fingerprint to be generated");
                return null;
            }

            var avTrack = new AVTrack(prefixed != null ? new AudioTrack(prefixed) : null, videoTrack);
            var hashes = await CreateQueryFingerprints(fingerprintCommandBuilder, avTrack);
            logger.LogDebug("Created hashes {Hashes} from aggregated track {AVTrack}", hashes, avTrack);
            return hashes;
        }
        
        private AVTrack GetAvTrack(string file, MediaType mediaType)
        {
            if (mediaType.HasFlag(MediaType.Audio | MediaType.Video))
            {
                if (mediaService == null)
                {
                    throw new ArgumentException("Set IMediaService via UsingServices method to be able to create audio and video fingerprints");
                }

                return mediaService.ReadAVTrackFromFile(file, configuration.QueryConfiguration.FingerprintConfiguration.GetTrackReadConfiguration(), mediaType);
            }

            if (mediaType.HasFlag(MediaType.Video))
            {
                var service = mediaService ?? videoService;
                if (service == null)
                {
                    throw new ArgumentException("Set IMediaService or IVideoService via UsingServices method to be able to create video fingerprints");
                }

                var frames = service.ReadFramesFromFile(file, configuration.QueryConfiguration.FingerprintConfiguration.GetTrackReadConfiguration().VideoConfig);
                return new AVTrack(null, new VideoTrack(frames));
            }

            var audioServiceToUse = mediaService ?? audioService;

            var samples = audioServiceToUse.ReadMonoSamplesFromFile(file, configuration.QueryConfiguration.Audio.FingerprintConfiguration.SampleRate);
            return new AVTrack(new AudioTrack(samples), null);
        }

        private async Task<AVHashes> CreateQueryFingerprints(IFingerprintCommandBuilder commandBuilder, AVTrack avTrack)
        {
            return await commandBuilder
                .BuildFingerprintCommand()
                .From(avTrack)
                .WithFingerprintConfig(configuration.QueryConfiguration.FingerprintConfiguration)
                .Hash();
        }

        /// <summary>
        ///  Gets an instance of <see cref="AVQueryResult"/> with query results from both Audio and Video queries.
        /// </summary>
        /// <param name="hashes">An instance of <see cref="AVHashes"/>.</param>
        /// <returns>An instance of <see cref="AVQueryResult"/>.</returns>
        /// <exception cref="IOException">Input/Output exception when querying the model service.</exception>> 
        private async Task<AVQueryResult> GetAvQueryResult(AVHashes hashes)
        {
            var queryResult = await queryCommandBuilder
                .BuildQueryCommand()
                .From(hashes)
                .WithQueryConfig(configuration.QueryConfiguration)
                .UsingServices(modelService)
                .Query();
            var avQueryResult = queryResultInterceptor(queryResult);
            queryLength += (hashes.Audio?.DurationInSeconds + hashes.Audio?.TimeOffset ?? hashes.Video?.DurationInSeconds) ?? 0;
            return avQueryResult;
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