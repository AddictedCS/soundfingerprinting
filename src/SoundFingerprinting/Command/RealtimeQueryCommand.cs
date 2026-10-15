namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
                _ => Task.CompletedTask,
                _ => Task.CompletedTask, (e, _) => throw e, () => {/* do nothing */ });
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

        /// <inheritdoc cref="IRealtimeQueryCommand.QueryAsync"/>
        public async Task<double> QueryAsync(CancellationToken cancellationToken)
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
                new ResultEntryConcatenator(loggerFactory, configuration.SeamJumpPolicy),
                new ResultEntryConcatenator(loggerFactory, configuration.SeamJumpPolicy),
                configuration.IncludeQueryHashesInResponse ? new StatefulQueryHashesConcatenator() : new NoOpQueryHashesConcatenator(loggerFactory),
                loggerFactory);

            try
            {
                // the offline storage is drained at startup; a query failure is handled inside the drain and the realtime loop continues
                await TryDrainOfflineStorageAsync(liveHashes: null, resultsAggregator, cancellationToken);
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
            {
                throw;
            }
            catch (Exception e)
            {
                // a callback failed on an already acknowledged chunk: report it and continue with the realtime source
                HandleQueryFailure(null, e);
            }

            while (true)
            {
                try
                {
                    await QueryFromRealtimeAndOffline(realtimeHashes, resultsAggregator, cancellationToken);
                    logger.LogInformation("Stopped querying realtime source after {QueryLength:00} seconds", queryLength);
                    await PurgeResultEntryAggregatorAsync(resultsAggregator);
                    return queryLength;
                }
                catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
                {
                    await PurgeResultEntryAggregatorAsync(resultsAggregator);
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
                        await PurgeResultEntryAggregatorAsync(resultsAggregator);
                        throw;
                    }

                    try
                    {
                        await Task.Delay(configuration.ErrorBackoffPolicy.RemainingDelay, cancellationToken);
                    }
                    catch (Exception delayException) when (delayException is OperationCanceledException or ObjectDisposedException)
                    {
                        // cancellation during the backoff delay has to deliver pending aggregated entries, same as cancellation at any other point
                        await PurgeResultEntryAggregatorAsync(resultsAggregator);
                        throw;
                    }
                }
            }
        }

        private async Task PurgeResultEntryAggregatorAsync(IRealtimeResultEntryAggregator resultsAggregator)
        {
            var purged = resultsAggregator.Purge();
            await InvokeCallbackHandlerAsync(purged.SuccessEntries, configuration.SuccessCallback);
            await InvokeCallbackHandlerAsync(purged.DidNotPassThresholdEntries, configuration.DidNotPassFilterCallback);
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
                await TryQueryAsync(intercepted, resultsAggregator, cancellationToken);
            }
        }

        private async Task ConsumeQueryResultAsync(AVQueryResult queryResult, IRealtimeResultEntryAggregator resultsAggregator)
        {
            var (ongoingEntries, successEntries, didNotPassThresholdEntries) = resultsAggregator.Consume(queryResult);
            await configuration.OngoingCallback(queryResult.StreamId, ongoingEntries);
            await InvokeCallbackHandlerAsync(successEntries, configuration.SuccessCallback);
            await InvokeCallbackHandlerAsync(didNotPassThresholdEntries, configuration.DidNotPassFilterCallback);
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
        ///  Drains the offline storage ahead of the live chunk: every stored chunk is queried, acknowledged and consumed in order.
        /// </summary>
        /// <param name="liveHashes">Live chunk that waits behind the drain; a query failure reroutes it to the offline storage. Null at startup.</param>
        /// <param name="resultsAggregator">Results aggregator to use for query results aggregation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True when the storage is drained, false when a query failed and the drain stopped.</returns>
        /// <exception cref="OperationCanceledException">Operation cancelled by the caller.</exception>
        /// <exception cref="ObjectDisposedException">Object disposed exception (invoked on cancellation token).</exception>
        /// <exception cref="Exception">An exception thrown by a user callback; the chunk that triggered it is already consumed and never replayed.</exception>
        private async Task<bool> TryDrainOfflineStorageAsync(AVHashes? liveHashes, IRealtimeResultEntryAggregator resultsAggregator, CancellationToken cancellationToken)
        {
            var offlineStorage = configuration.OfflineStorage;
            while (offlineStorage.Any())
            {
                var offlineHashes = offlineStorage.First();
                AVQueryResult offlineResult;
                try
                {
                    logger.LogDebug("Read AVHashes from offline storage {Hashes}. Querying storage", offlineHashes);
                    offlineResult = await GetAvQueryResultAsync(offlineHashes, cancellationToken);
                    MarkRestoredFromError();
                }
                catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // the server is not reachable: the stored chunk stays for the next attempt and the live chunk queues behind it
                    HandleQueryFailure(liveHashes, e);
                    return false;
                }

                // the chunk is acknowledged as soon as its query succeeded: a callback failure below must not replay it
                offlineStorage.Remove(offlineHashes);
                await ConsumeQueryResultAsync(offlineResult, resultsAggregator);
                await Task.Delay(configuration.DelayStrategy.Delay, cancellationToken);
            }

            return true;
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
        private async Task<bool> TryQueryAsync(AVHashes hashes, IRealtimeResultEntryAggregator realtimeAggregator, CancellationToken cancellationToken)
        {
            // the offline storage drains BEFORE the live chunk: every query makes the server publish the
            // chunk it received, so querying the live chunk first hands the consumers the newest chunk
            // ahead of the outage chunks, and a consumer that guards on its own stream end drops them
            try
            {
                if (!await TryDrainOfflineStorageAsync(hashes, realtimeAggregator, cancellationToken))
                {
                    return false;
                }
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
            {
                throw;
            }
            catch (Exception e)
            {
                // a callback failed on a drained chunk that is already acknowledged; the live chunk behind the drain is
                // still unqueried, so it goes to the offline storage and is queried exactly once later
                HandleQueryFailure(hashes, e);
                return false;
            }

            AVQueryResult avQueryResult;
            try
            {
                var (audioMilliseconds, videoMilliseconds) = hashes.FingerprintingTime;
                avQueryResult = (await GetAvQueryResultAsync(hashes, cancellationToken)).WithFingerprintingDurationMilliseconds(audioMilliseconds, videoMilliseconds);
                MarkRestoredFromError();
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
            {
                throw;
            }
            catch (Exception e)
            {
                // here we catch exceptions that are related to server not reachable; the current chunk was
                // not consumed yet inside the try block, so rerouting it to the offline storage
                // is always safe (it will be queried and consumed exactly once, later)
                HandleQueryFailure(hashes, e);
                return false;
            }

            // consuming the current chunk stays outside the try block: an exception thrown by a user callback
            // must not reroute already consumed hashes to the offline storage, as re-querying them later would
            // emit the same results a second time and double-count the query length
            await ConsumeQueryResultAsync(avQueryResult, realtimeAggregator);
            return true;
        }

        /// <summary>
        ///  Gets an instance of <see cref="AVQueryResult"/> with query results from both Audio and Video queries.
        /// </summary>
        /// <param name="hashes">An instance of <see cref="AVHashes"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An instance of <see cref="AVQueryResult"/>.</returns>
        /// <exception cref="IOException">Input/Output exception when querying the model service.</exception>>
        private async Task<AVQueryResult> GetAvQueryResultAsync(AVHashes hashes, CancellationToken cancellationToken)
        {
            var queryResult = await queryCommandBuilder
                .BuildQueryCommand()
                .From(hashes)
                .WithQueryConfig(configuration.QueryConfiguration)
                .UsingServices(queryService)
                .QueryAsync(cancellationToken);
            var avQueryResult = queryResultInterceptor(queryResult);
            queryLength += (hashes.Audio?.DurationInSeconds + hashes.Audio?.TimeOffset ?? hashes.Video?.DurationInSeconds) ?? 0;
            return avQueryResult;
        }

        private static async Task InvokeCallbackHandlerAsync(IEnumerable<AVQueryResult> results, Func<AVQueryResult, Task> callback)
        {
            foreach (var queryResult in results)
            {
                await callback(queryResult);
            }
        }
    }
}
