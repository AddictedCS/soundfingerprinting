namespace SoundFingerprinting.Command
{
    using System;
    using System.Diagnostics;
    using System.Linq;
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
    ///  Fingerprint command class that configures and builds the fingerprints from a source content file.
    /// </summary>
    /// <remarks>
    ///  Create and configure with <see cref="FingerprintCommandBuilder"/> class.
    /// </remarks>
    public sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration
    {
        private readonly IFingerprintService fingerprintService;
        private readonly ILogger<FingerprintCommand> logger;

        private Func<AVHashes> createFingerprintsMethod;
        
        private IAudioService audioService;
        private IVideoService? videoService;
        private IMediaService? mediaService;

        private AVFingerprintConfiguration fingerprintConfiguration;

        internal FingerprintCommand(IFingerprintService fingerprintService, ILoggerFactory loggerFactory)
        {
            this.fingerprintService = fingerprintService;
            audioService = new SoundFingerprintingAudioService();
            fingerprintConfiguration = new DefaultAVFingerprintConfiguration();
            createFingerprintsMethod = () => AVHashes.Empty;
            logger = loggerFactory.CreateLogger<FingerprintCommand>();
        }

        /// <inheritdoc cref="IFingerprintCommand.Hash"/>
        public Task<AVHashes> Hash()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        /// <inheritdoc cref="ISourceFrom.From(string, MediaType)"/>
        public IWithFingerprintConfiguration From(string file, MediaType mediaType = MediaType.Audio)
        {
            return From(file, 0d, 0d, mediaType);
        }

        /// <inheritdoc cref="ISourceFrom.From(AudioSamples)"/>
        public IWithFingerprintConfiguration From(AudioSamples audioSamples)
        {
            createFingerprintsMethod = () => GetAvHashes(audioSamples);
            return this;
        }

        /// <inheritdoc cref="ISourceFrom.From(Frames)"/>
        public IWithFingerprintConfiguration From(Frames frames)
        {
            createFingerprintsMethod = () => GetAvHashes(frames);
            return this;
        }

        /// <inheritdoc cref="ISourceFrom.From(AVTrack)"/>
        public IWithFingerprintConfiguration From(AVTrack avTrack)
        {
            createFingerprintsMethod = () => GetAvHashes(avTrack);
            return this;
        }

        /// <inheritdoc cref="ISourceFrom.From(string,double,double,MediaType)"/>
        public IWithFingerprintConfiguration From(string file, double secondsToProcess, double startAtSecond, MediaType mediaType = MediaType.Audio)
        {
            createFingerprintsMethod = () =>
            {
                if (mediaType.HasFlag(MediaType.Video) && videoService == null && mediaService == null)
                {
                    throw new ArgumentException("You need to specify a valid instance of IVideoService or IMediaService if you want to process video content from the file.");
                }
                
                // both audio and video are requested.
                if (mediaType.HasFlag(MediaType.Audio | MediaType.Video))
                {
                    if (mediaService != null)
                    {
                        logger.LogDebug("Using media service {0} to read AVTrack from file {1}", mediaService, file);
                        var avTrack = mediaService.ReadAVTrackFromFile(file, fingerprintConfiguration.GetTrackReadConfiguration(), secondsToProcess, startAtSecond, mediaType);
                        return GetAvHashes(avTrack);
                    }
                    
                    var audioSamples = audioService.ReadMonoSamplesFromFile(file, fingerprintConfiguration.Audio.SampleRate, secondsToProcess, startAtSecond);
                    var frames = videoService!.ReadFramesFromFile(file, fingerprintConfiguration.GetTrackReadConfiguration().VideoConfig, secondsToProcess, startAtSecond);
                    return GetAvHashes(audioSamples, frames);
                }

                if (mediaType.HasFlag(MediaType.Audio))
                {
                    // media service has precedence since audio service is set by default.
                    var serviceToUse = mediaService ?? audioService;
                    logger.LogDebug("Using service {Service} to read audio samples from file {File}", serviceToUse, file);
                    AudioSamples audioSamples = serviceToUse.ReadMonoSamplesFromFile(file, fingerprintConfiguration.Audio.SampleRate, secondsToProcess, startAtSecond);
                    return GetAvHashes(audioSamples); 
                }

                if (mediaType.HasFlag(MediaType.Video))
                {
                    // media service is used by default.
                    var serviceToUse = mediaService ?? videoService;
                    logger.LogDebug("Using service {Service} to read frames from file {File}", serviceToUse, file);
                    var frames = serviceToUse!.ReadFramesFromFile(file, fingerprintConfiguration.GetTrackReadConfiguration().VideoConfig, secondsToProcess, startAtSecond);
                    return GetAvHashes(frames);
                }
                
                logger.LogError("Unknown media type specified in fingerprint creation method {MediaType}. Returning empty hashes", mediaType);
                return AVHashes.Empty;
            };

            return this;
        }

        /// <inheritdoc cref="IWithFingerprintConfiguration.WithFingerprintConfig(AVFingerprintConfiguration)"/>
        public IUsingFingerprintServices WithFingerprintConfig(AVFingerprintConfiguration configuration)
        {
            fingerprintConfiguration = configuration;
            return this;
        }

        /// <inheritdoc cref="IWithFingerprintConfiguration.WithFingerprintConfig(Func{AVFingerprintConfiguration,AVFingerprintConfiguration})"/>
        public IUsingFingerprintServices WithFingerprintConfig(Func<AVFingerprintConfiguration, AVFingerprintConfiguration> amendFunctor)
        {
            fingerprintConfiguration = amendFunctor(fingerprintConfiguration);
            return this;
        }

        /// <inheritdoc cref="IUsingFingerprintServices.UsingServices(IAudioService)"/>
        public IFingerprintCommand UsingServices(IAudioService audioService)
        {
            this.audioService = audioService;
            return this;
        }

        /// <inheritdoc cref="IUsingFingerprintServices.UsingServices(IVideoService)"/>
        public IFingerprintCommand UsingServices(IVideoService videoService)
        {
            this.videoService = videoService;
            return this;
        }

        /// <inheritdoc cref="IUsingFingerprintServices.UsingServices(IMediaService)"/>
        public IFingerprintCommand UsingServices(IMediaService mediaService)
        {
            this.mediaService = mediaService;
            return this;
        }

        private AVHashes GetAvHashes(AVTrack avTrack)
        {
            long audioElapsedMilliseconds = 0, videoElapsedMilliseconds = 0;
            var (audioTrack, videoTrack) = avTrack;
            var audio = audioTrack != null ? GetHashes(audioTrack.Samples, out audioElapsedMilliseconds) : null;
            var video = videoTrack != null ? GetHashes(videoTrack.Frames, out videoElapsedMilliseconds) : null;
            return new AVHashes(audio?.Hashes, video?.Hashes, new AVFingerprintingTime(audioElapsedMilliseconds, videoElapsedMilliseconds));
        }

        private AVHashes GetAvHashes(AudioSamples audioSamples, Frames frames)
        {
            var audio = GetHashes(audioSamples, out long audioElapsedMilliseconds);
            var video = GetHashes(frames, out long videoElapsedMilliseconds);
            return new AVHashes(audio.Hashes, video.Hashes, new AVFingerprintingTime(audioElapsedMilliseconds, videoElapsedMilliseconds));
        }

        private AVHashes GetAvHashes(AudioSamples audioSamples)
        {
            var audio = GetHashes(audioSamples, out long elapsedMilliseconds);
            return new AVHashes(audio.Hashes, null, new AVFingerprintingTime(elapsedMilliseconds, 0));
        }

        private AVHashes GetAvHashes(Frames frames)
        {
            var video = GetHashes(frames, out long elapsedMilliseconds);
            return new AVHashes(null, video.Hashes, new AVFingerprintingTime(0, elapsedMilliseconds));
        }
        
        private FingerprintsAndHashes GetHashes(AudioSamples audioSamples, out long elapsedMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew(); 
            var hashes = fingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, fingerprintConfiguration.Audio);
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            return hashes;
        }

        private FingerprintsAndHashes GetHashes(Frames frames, out long elapsedMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew(); 
            var videoHashes = fingerprintService.CreateFingerprintsFromImageFrames(frames, fingerprintConfiguration.Video);
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            return videoHashes;
        }
    }
}