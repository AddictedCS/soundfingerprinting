namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Generic;
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
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Realtime fingerprint command that turns a realtime source into a sequence of <see cref="AVHashes"/>.
    /// </summary>
    /// <remarks>
    ///  Create and configure with <see cref="FingerprintCommandBuilder.BuildRealtimeFingerprintCommand"/>.
    /// </remarks>
    public sealed class RealtimeFingerprintCommand : IRealtimeFingerprintSource, IWithRealtimeFingerprintConfiguration
    {
        private readonly ILogger<RealtimeFingerprintCommand> logger;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;

        private AVFingerprintConfiguration fingerprintConfiguration = new DefaultAVFingerprintConfiguration();
        private Func<CancellationToken, IAsyncEnumerable<RealtimeElement>> source;
        private IAudioService audioService = new SoundFingerprintingAudioService();
        private IVideoService? videoService;
        private IMediaService? mediaService;
        private IRealtimeMediaService? realtimeMediaService;
        private Func<AVTrack, AVTrack> avTrackInterceptor = _ => _;
        private Func<AVHashes, AVHashes> hashesInterceptor = _ => _;

        internal RealtimeFingerprintCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, ILoggerFactory loggerFactory)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            logger = loggerFactory.CreateLogger<RealtimeFingerprintCommand>();
            source = _ => throw new ArgumentException("Select a realtime source with one of the From methods");
        }

        /// <inheritdoc cref="IRealtimeFingerprintSource.From(string,double,MediaType)"/>
        public IWithRealtimeFingerprintConfiguration From(string url, double chunkLength, MediaType mediaType = MediaType.Audio)
        {
            source = cancellationToken =>
            {
                if (realtimeMediaService == null)
                {
                    throw new ArgumentException("Set an instance of IRealtimeMediaService in UsingServices method to be able to generate fingerprints directly from broadcast URL");
                }

                var tracks = realtimeMediaService.ReadAVTrackFromRealtimeSource(url, chunkLength, fingerprintConfiguration.GetTrackReadConfiguration(), mediaType, cancellationToken);
                return Map(tracks, track => new RealtimeElement(track, null), cancellationToken);
            };

            return this;
        }

        /// <inheritdoc cref="IRealtimeFingerprintSource.From(IAsyncEnumerable{AudioSamples})"/>
        public IWithRealtimeFingerprintConfiguration From(IAsyncEnumerable<AudioSamples> source)
        {
            this.source = cancellationToken => Map(source, samples => new RealtimeElement(new AVTrack(new AudioTrack(samples), null), null), cancellationToken);
            return this;
        }

        /// <inheritdoc cref="IRealtimeFingerprintSource.From(IAsyncEnumerable{string},MediaType)"/>
        public IWithRealtimeFingerprintConfiguration From(IAsyncEnumerable<string> files, MediaType mediaType = MediaType.Audio)
        {
            source = cancellationToken => Map(files, file => new RealtimeElement(GetAvTrack(file, mediaType), null), cancellationToken);
            return this;
        }

        /// <inheritdoc cref="IRealtimeFingerprintSource.From(IAsyncEnumerable{StreamingFile})"/>
        public IWithRealtimeFingerprintConfiguration From(IAsyncEnumerable<StreamingFile> files)
        {
            source = cancellationToken => Map(files, file =>
            {
                logger.LogDebug("Consuming streaming file {File}", file);
                // the file stamp is the stamp of the chunk; it is applied before interception so an interceptor sees the capture time
                return new RealtimeElement(GetAvTrack(file.Path, file.MediaType).WithRelativeTo(file.RelativeTo), file.StreamId);
            }, cancellationToken);
            return this;
        }

        /// <inheritdoc cref="IRealtimeFingerprintSource.From(IAsyncEnumerable{AVTrack})"/>
        public IWithRealtimeFingerprintConfiguration From(IAsyncEnumerable<AVTrack> tracks)
        {
            source = cancellationToken => Map(tracks, track => new RealtimeElement(track, null), cancellationToken);
            return this;
        }

        /// <inheritdoc cref="IWithRealtimeFingerprintConfiguration.WithFingerprintConfig(AVFingerprintConfiguration)"/>
        public IInterceptRealtimeFingerprints WithFingerprintConfig(AVFingerprintConfiguration configuration)
        {
            fingerprintConfiguration = configuration;
            return this;
        }

        /// <inheritdoc cref="IWithRealtimeFingerprintConfiguration.WithFingerprintConfig(Func{AVFingerprintConfiguration,AVFingerprintConfiguration})"/>
        public IInterceptRealtimeFingerprints WithFingerprintConfig(Func<AVFingerprintConfiguration, AVFingerprintConfiguration> amendFunctor)
        {
            fingerprintConfiguration = amendFunctor(fingerprintConfiguration);
            return this;
        }

        /// <inheritdoc cref="IInterceptRealtimeFingerprints.InterceptAVTrack"/>
        public IInterceptRealtimeFingerprints InterceptAVTrack(Func<AVTrack, AVTrack> avTrackInterceptor)
        {
            this.avTrackInterceptor = avTrackInterceptor;
            return this;
        }

        /// <inheritdoc cref="IInterceptRealtimeFingerprints.InterceptHashes"/>
        public IInterceptRealtimeFingerprints InterceptHashes(Func<AVHashes, AVHashes> hashesInterceptor)
        {
            this.hashesInterceptor = hashesInterceptor;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeFingerprintServices.UsingServices(IAudioService)"/>
        public IRealtimeFingerprintCommand UsingServices(IAudioService audioService)
        {
            this.audioService = audioService;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeFingerprintServices.UsingServices(IVideoService)"/>
        public IRealtimeFingerprintCommand UsingServices(IVideoService videoService)
        {
            this.videoService = videoService;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeFingerprintServices.UsingServices(IMediaService)"/>
        public IRealtimeFingerprintCommand UsingServices(IMediaService mediaService)
        {
            this.mediaService = mediaService;
            return this;
        }

        /// <inheritdoc cref="IUsingRealtimeFingerprintServices.UsingServices(IRealtimeMediaService)"/>
        public IRealtimeFingerprintCommand UsingServices(IRealtimeMediaService realtimeMediaService)
        {
            this.realtimeMediaService = realtimeMediaService;
            return this;
        }

        /// <inheritdoc cref="IRealtimeFingerprintCommand.Hash"/>
        public IAsyncEnumerable<AVHashes> Hash(CancellationToken cancellationToken)
        {
            var aggregator = new RealtimeAudioSamplesAggregator(
                fingerprintConfiguration.Audio.SpectrogramConfig.MinimumSamplesPerFingerprint,
                fingerprintConfiguration.Audio.Stride);
            return new RealtimeHashes(this, aggregator, cancellationToken);
        }

        private static async IAsyncEnumerable<RealtimeElement> Map<T>(IAsyncEnumerable<T> source, Func<T, RealtimeElement> map, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                yield return map(item);
            }
        }

        private async IAsyncEnumerable<AVHashes> Enumerate(RealtimeAudioSamplesAggregator aggregator, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var element in source(cancellationToken).WithCancellation(cancellationToken))
            {
                var hashes = await CreateHashes(element, aggregator);
                if (hashes == null)
                {
                    continue;
                }

                yield return hashesInterceptor(hashes);
            }
        }

        private async Task<AVHashes?> CreateHashes(RealtimeElement element, RealtimeAudioSamplesAggregator aggregator)
        {
            logger.LogDebug("Retrieved track {Track} from realtime source", element.Track);
            var (audioTrack, videoTrack) = avTrackInterceptor(element.Track);
            var prefixed = audioTrack != null ? aggregator.Aggregate(audioTrack.Samples) : null;
            if (prefixed == null && videoTrack == null)
            {
                // can happen when there is not enough samples to generate a fingerprint
                logger.LogDebug("Both audio and video tracks are null or empty. Waiting until minimum number of samples are aggregated for a fingerprint to be generated");
                return null;
            }

            var avTrack = new AVTrack(prefixed != null ? new AudioTrack(prefixed) : null, videoTrack);
            var hashes = await fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(avTrack)
                .WithFingerprintConfig(fingerprintConfiguration)
                .Hash();
            logger.LogDebug("Created hashes {Hashes} from aggregated track {AVTrack}", hashes, avTrack);
            return element.StreamId != null ? hashes.WithStreamId(element.StreamId) : hashes;
        }

        private AVTrack GetAvTrack(string file, MediaType mediaType)
        {
            var readConfiguration = fingerprintConfiguration.GetTrackReadConfiguration();
            if (mediaType.HasFlag(MediaType.Audio | MediaType.Video))
            {
                if (mediaService == null)
                {
                    throw new ArgumentException("Set IMediaService via UsingServices method to be able to create audio and video fingerprints");
                }

                return mediaService.ReadAVTrackFromFile(file, readConfiguration, mediaType);
            }

            if (mediaType.HasFlag(MediaType.Video))
            {
                var service = mediaService ?? videoService;
                if (service == null)
                {
                    throw new ArgumentException("Set IMediaService or IVideoService via UsingServices method to be able to create video fingerprints");
                }

                var frames = service.ReadFramesFromFile(file, readConfiguration.VideoConfig);
                return new AVTrack(null, new VideoTrack(frames));
            }

            var audioServiceToUse = mediaService ?? audioService;
            var samples = audioServiceToUse.ReadMonoSamplesFromFile(file, fingerprintConfiguration.Audio.SampleRate);
            return new AVTrack(new AudioTrack(samples), null);
        }

        /// <summary>
        ///  One element of the realtime source: the track and, for a streaming file, the stream id.
        /// </summary>
        private sealed class RealtimeElement
        {
            public RealtimeElement(AVTrack track, string? streamId)
            {
                Track = track;
                StreamId = streamId;
            }

            public AVTrack Track { get; }

            public string? StreamId { get; }
        }

        /// <summary>
        ///  Hot enumerable that owns the samples aggregator of one run and re-enumerates the source on every enumeration.
        /// </summary>
        private sealed class RealtimeHashes : IAsyncEnumerable<AVHashes>
        {
            private readonly RealtimeFingerprintCommand command;
            private readonly RealtimeAudioSamplesAggregator aggregator;
            private readonly CancellationToken cancellationToken;

            public RealtimeHashes(RealtimeFingerprintCommand command, RealtimeAudioSamplesAggregator aggregator, CancellationToken cancellationToken)
            {
                this.command = command;
                this.aggregator = aggregator;
                this.cancellationToken = cancellationToken;
            }

            public IAsyncEnumerator<AVHashes> GetAsyncEnumerator(CancellationToken enumeratorToken = default)
            {
                if (!enumeratorToken.CanBeCanceled || enumeratorToken == cancellationToken)
                {
                    return command.Enumerate(aggregator, cancellationToken).GetAsyncEnumerator(cancellationToken);
                }

                if (!cancellationToken.CanBeCanceled)
                {
                    return command.Enumerate(aggregator, enumeratorToken).GetAsyncEnumerator(enumeratorToken);
                }

                return EnumerateLinked(enumeratorToken).GetAsyncEnumerator(enumeratorToken);
            }

            private async IAsyncEnumerable<AVHashes> EnumerateLinked([EnumeratorCancellation] CancellationToken enumeratorToken)
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, enumeratorToken);
                await foreach (var hashes in command.Enumerate(aggregator, linked.Token).WithCancellation(linked.Token))
                {
                    yield return hashes;
                }
            }
        }
    }
}
