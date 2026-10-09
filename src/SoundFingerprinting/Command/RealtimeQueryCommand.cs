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
    /// <remarks>
    ///  The hashes come from a <see cref="RealtimeFingerprintCommand"/> built with the same source, fingerprint configuration, track interceptor and services.
    /// </remarks>
    public sealed class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration
    {
        private readonly ILogger<RealtimeQueryCommand> logger;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryCommandBuilder queryCommandBuilder;
        private readonly ILoggerFactory loggerFactory;

        private Func<IRealtimeFingerprintSource, IWithRealtimeFingerprintConfiguration>? sourceSelector;
        private IAsyncEnumerable<AVHashes>? hashesSource;
        private RealtimeQueryConfiguration configuration;
        private IRealtimeMediaService? realtimeMediaService;
        private IQueryService? queryService;
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
            hashesSource = new BlockingRealtimeCollection<AVHashes>(new BlockingCollection<AVHashes>());
            audioService = new SoundFingerprintingAudioService();
        }

        /// <inheritdoc cref="IRealtimeSource.From(string,double,MediaType)"/>
        public IWithRealtimeQueryConfiguration From(string url, double chunkLength, MediaType mediaType = MediaType.Audio)
        {
            return Select(source => source.From(url, chunkLength, mediaType));
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AudioSamples})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> source)
        {
            return Select(realtimeSource => realtimeSource.From(source));
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{string},MediaType)"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<string> files, MediaType mediaType = MediaType.Audio)
        {
            return Select(source => source.From(files, mediaType));
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{StreamingFile})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<StreamingFile> files)
        {
            return Select(source => source.From(files));
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AVTrack})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AVTrack> tracks)
        {
            return Select(source => source.From(tracks));
        }

        /// <inheritdoc cref="IRealtimeSource.From(IAsyncEnumerable{AVHashes})"/>
        public IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AVHashes> avHashes)
        {
            hashesSource = avHashes;
            sourceSelector = null;
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

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IQueryService)"/>
        public IRealtimeQueryCommand UsingServices(IQueryService service)
        {
            queryService = service;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IQueryService,IAudioService)"/>
        public IRealtimeQueryCommand UsingServices(IQueryService modelService, IAudioService audioService)
        {
            this.queryService = modelService;
            this.audioService = audioService;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IQueryService,IMediaService)"/>
        public IRealtimeQueryCommand UsingServices(IQueryService modelService, IMediaService mediaService)
        {
            this.queryService = modelService;
            this.mediaService = mediaService;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IQueryService,IVideoService)"/>
        public IRealtimeQueryCommand UsingServices(IQueryService modelService, IVideoService videoService)
        {
            this.queryService = modelService;
            this.videoService = videoService;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeQueryServices.UsingServices(IQueryService,IRealtimeMediaService)"/>
        public IRealtimeQueryCommand UsingServices(IQueryService modelService, IRealtimeMediaService realtimeMediaService)
        {
            this.queryService = modelService;
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

        private IWithRealtimeQueryConfiguration Select(Func<IRealtimeFingerprintSource, IWithRealtimeFingerprintConfiguration> selector)
        {
            sourceSelector = selector;
            hashesSource = null;
            return this;
        }

        /// <summary>
        ///  Builds the realtime hashes of this run: the fingerprint command gets the source, the query fingerprint configuration, the track interceptor and the services of this command.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Hot enumerable of realtime hashes that keeps its samples aggregator across re-enumerations.</returns>
        private IAsyncEnumerable<AVHashes> BuildRealtimeHashes(CancellationToken cancellationToken)
        {
            if (hashesSource != null)
            {
                return hashesSource;
            }

            var command = sourceSelector!(fingerprintCommandBuilder.BuildRealtimeFingerprintCommand())
                .WithFingerprintConfig(configuration.QueryConfiguration.FingerprintConfiguration)
                .InterceptAVTrack(avTrackInterceptor);

            var withServices = realtimeMediaService != null ? command.UsingServices(realtimeMediaService)
                : mediaService != null ? command.UsingServices(mediaService)
                : videoService != null ? command.UsingServices(videoService)
                : command.UsingServices(audioService);
            return withServices.Hash(cancellationToken);
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
            // one hashes enumerable per run: a retry re-enumerates it, so the buffered tail survives a transient error
            var realtimeHashes = BuildRealtimeHashes(cancellationToken);
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(
                configuration.ResultEntryFilter,
                configuration.OngoingResultEntryFilter,
                new AvResultEntryCompletionStrategy(configuration.QueryConfiguration, configuration.OngoingEntryStalenessCeiling),
                new ResultEntryConcatenator(loggerFactory, configuration.AutomaticSkipDetection),
                new ResultEntryConcatenator(loggerFactory, configuration.AutomaticSkipDetection),
                configuration.IncludeQueryHashesInResponse ? new StatefulQueryHashesConcatenator() : new NoOpQueryHashesConcatenator(loggerFactory),
                loggerFactory);

            try
            {
                // let's check the offline storage immediately at startup
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
                    await QueryFromRealtimeAndOffline(realtimeHashes, resultsAggregator, cancellationToken);
                    logger.LogInformation("Stopped querying realtime source after {QueryLength:00} seconds", queryLength);
                    PurgeResultEntryAggregator(resultsAggregator);
                    return queryLength;
                }
                catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
                {
                    PurgeResultEntryAggregator(resultsAggregator);
                    throw;
                }
                catch (Exception e)
                {
                    try
                    {
                        // here we catch exceptions that occur while reading from the realtime source
                        HandleQueryFailure(null, e);
                    }
                    catch (Exception e2)
                    {
                        logger.LogWarning(e2, "Failed to handle query failure. Purging realtime aggregator, exiting query loop");
                        PurgeResultEntryAggregator(resultsAggregator);
                        throw;
                    }

                    try
                    {
                        await Task.Delay(configuration.ErrorBackoffPolicy.RemainingDelay, cancellationToken);
                    }
                    catch (Exception delayException) when (delayException is OperationCanceledException or ObjectDisposedException)
                    {
                        // cancellation during the backoff delay has to deliver pending aggregated entries, same as cancellation at any other point
                        PurgeResultEntryAggregator(resultsAggregator);
                        throw;
                    }
                }
            }
        }

        private void PurgeResultEntryAggregator(IRealtimeResultEntryAggregator resultsAggregator)
        {
            var purged = resultsAggregator.Purge();
            InvokeCallbackHandler(purged.SuccessEntries, configuration?.SuccessCallback);
            InvokeCallbackHandler(purged.DidNotPassThresholdEntries, configuration?.DidNotPassFilterCallback);
        }

        /// <summary>
        ///  Query from realtime and offline sources.
        /// </summary>
        /// <param name="realtimeHashes">Realtime hashes of this run.</param>
        /// <param name="resultsAggregator">Results aggregator to use for query results aggregation.</param>
        /// <param name="cancellationToken">Cancellation token to cancel querying.</param>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        /// <exception cref="Exception">Any other exception that can occur during fingerprinting creation, and not fingerprinting querying.</exception>
        private async Task QueryFromRealtimeAndOffline(IAsyncEnumerable<AVHashes> realtimeHashes, IRealtimeResultEntryAggregator resultsAggregator, CancellationToken cancellationToken)
        {
            await foreach (var avHashes in realtimeHashes.WithCancellation(cancellationToken))
            {
                var intercepted = hashesInterceptor(avHashes);
                logger.LogDebug("Issuing query with hashes {Hashes}", intercepted);
                await TryQuery(intercepted, resultsAggregator, cancellationToken);
            }
        }

        private void ConsumeQueryResult(AVQueryResult queryResult, IRealtimeResultEntryAggregator resultsAggregator)
        {
            var (ongoingEntries, successEntries, didNotPassThresholdEntries) = resultsAggregator.Consume(queryResult);
            InvokeOngoingCallbackHandler(queryResult.StreamId, ongoingEntries);
            InvokeCallbackHandler(successEntries, configuration.SuccessCallback);
            InvokeCallbackHandler(didNotPassThresholdEntries, configuration.DidNotPassFilterCallback);
        }

        /// <summary>
        ///  Clears the error state after a successful query. It runs on the first query that succeeds, drained
        ///  or live, so a long drain does not keep every consumer of the callback reporting a failed stream.
        /// </summary>
        private void MarkRestoredFromError()
        {
            if (!errored)
            {
                return;
            }

            logger.LogDebug("Query restored from previous error");
            errored = false;
            configuration.ErrorBackoffPolicy.Success();
            configuration.RestoredAfterErrorCallback();
        }

        private void HandleQueryFailure(AVHashes? hashes, Exception e)
        {
            errored = true;

            // backoff accounting and offline persistence must complete before the user callback runs:
            // the default ErrorCallback rethrows, and hashes would otherwise be lost
            configuration.ErrorBackoffPolicy.Failure();
            configuration.OfflineStorage.Add(hashes);
            configuration.ErrorCallback(e, hashes);
        }

        /// <summary>
        ///  Queries local or remote <see cref="IQueryService"/> with <see cref="AVHashes"/> gathered from offline storage.
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
                var offlineResult = await GetAvQueryResult(offlineHashes);
                MarkRestoredFromError();
                yield return offlineResult;
                offlineStorage.Remove(offlineHashes);
                await Task.Delay(configuration.DelayStrategy.Delay, cancellationToken);
            }
        }

        /// <summary>
        ///  Tries querying associated <see cref="IQueryService"/>.
        /// </summary>
        /// <param name="hashes">An instance of <see cref="AVHashes"/>.</param>
        /// <param name="realtimeAggregator">Realtime aggregator.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if query was successful, otherwise false.</returns>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        private async Task<bool> TryQuery(AVHashes hashes, IRealtimeResultEntryAggregator realtimeAggregator, CancellationToken cancellationToken)
        {
            AVQueryResult avQueryResult;
            try
            {
                // the offline storage drains BEFORE the live chunk: every query makes the server publish the
                // chunk it received, so querying the live chunk first hands the consumers the newest chunk
                // ahead of the outage chunks, and a consumer that guards on its own stream end drops them
                await foreach (var offlineResult in QueryFromOfflineStorage(cancellationToken))
                {
                    ConsumeQueryResult(offlineResult, realtimeAggregator);
                }

                var (audioMilliseconds, videoMilliseconds) = hashes.FingerprintingTime;
                avQueryResult = (await GetAvQueryResult(hashes)).WithFingerprintingDurationMilliseconds(audioMilliseconds, videoMilliseconds);
                MarkRestoredFromError();
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
            {
                throw;
            }
            catch (Exception e)
            {
                // here we catch exceptions that are related to server not reachable; the current chunk was
                // not consumed yet at any point inside the try block, so rerouting it to the offline storage
                // is always safe (it will be queried and consumed exactly once, later)
                HandleQueryFailure(hashes, e);
                return false;
            }

            // consuming the current chunk stays outside the try block: an exception thrown by a user callback
            // must not reroute already consumed hashes to the offline storage, as re-querying them later would
            // emit the same results a second time and double-count the query length
            ConsumeQueryResult(avQueryResult, realtimeAggregator);
            return true;
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
                .UsingServices(queryService)
                .Query();
            var avQueryResult = queryResultInterceptor(queryResult);
            queryLength += (hashes.Audio?.DurationInSeconds + hashes.Audio?.TimeOffset ?? hashes.Video?.DurationInSeconds) ?? 0;
            return avQueryResult;
        }

        private void InvokeOngoingCallbackHandler(string streamId, IEnumerable<AVResultEntry> ongoingResultEntries)
        {
            configuration.OngoingCallback(streamId, ongoingResultEntries);
        }

        private static void InvokeCallbackHandler(IEnumerable<AVQueryResult> results, Action<AVQueryResult>? callback)
        {
            foreach (var queryResult in results)
            {
                callback?.Invoke(queryResult);
            }
        }
    }
}
