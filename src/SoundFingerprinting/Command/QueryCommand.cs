namespace SoundFingerprinting.Command
{
    using System;
    using System.Linq;
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
    ///  Query command.
    /// </summary>
    public sealed class QueryCommand : IQuerySource, IWithQueryConfiguration
    {
        private readonly ILogger<QueryCommand> logger;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private IModelService? modelService;
        private IAudioService audioService;
        private IVideoService? videoService;
        private IMediaService? mediaService;
        private IQueryMatchRegistry? queryMatchRegistry;
        private Func<AVHashes, AVHashes> hashesInterceptor;
        
        private Func<IWithFingerprintConfiguration>? createFingerprintCommand;

        private AVQueryConfiguration queryConfiguration;

        internal QueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<QueryCommand>();
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            queryConfiguration = new DefaultAVQueryConfiguration();
            this.audioService = new SoundFingerprintingAudioService();
            hashesInterceptor = _ => _;
        }

        /// <inheritdoc cref="IQuerySource.From(string,MediaType)"/>
        public IWithQueryConfiguration From(string file, MediaType mediaType = MediaType.Audio)
        {
            createFingerprintCommand = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(file, mediaType);
            return this;
        }

        /// <inheritdoc cref="IQuerySource.From(string,double,double,MediaType)"/>
        public IWithQueryConfiguration From(string file, double secondsToProcess, double startAtSecond, MediaType mediaType)
        {
            createFingerprintCommand = () => fingerprintCommandBuilder.BuildFingerprintCommand()
                                                                      .From(file, secondsToProcess, startAtSecond, mediaType);
            return this;
        }

        /// <inheritdoc cref="IQuerySource.From(AudioSamples)"/>
        public IWithQueryConfiguration From(AudioSamples audioSamples)
        {
            createFingerprintCommand = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(audioSamples);
            return this;
        }

        /// <inheritdoc cref="IQuerySource.From(Frames)"/>
        public IWithQueryConfiguration From(Frames frames)
        {
            createFingerprintCommand = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(frames);
            return this;
        }

        /// <inheritdoc cref="IQuerySource.From(AVTrack)"/>
        public IWithQueryConfiguration From(AVTrack avTrack)
        {
            createFingerprintCommand = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(avTrack);
            return this;
        }

        /// <inheritdoc cref="IQuerySource.From(AVHashes)"/>
        public IWithQueryConfiguration From(AVHashes hashes)
        {
            createFingerprintCommand = () => new ExecutedFingerprintCommand(hashes);
            return this;
        }

        /// <inheritdoc cref="IWithQueryConfiguration.WithQueryConfig(AVQueryConfiguration)"/>
        public IInterceptHashes WithQueryConfig(AVQueryConfiguration config)
        {
            queryConfiguration = config;
            return this;
        }

        /// <inheritdoc cref="IWithQueryConfiguration.WithQueryConfig(Func{AVQueryConfiguration,AVQueryConfiguration})"/>
        public IInterceptHashes WithQueryConfig(Func<AVQueryConfiguration, AVQueryConfiguration> amendQueryConfigFunctor)
        {
            queryConfiguration = amendQueryConfigFunctor(queryConfiguration);
            return this;
        }

        /// <inheritdoc cref="IUsingQueryServices.UsingServices(IModelService)"/>
        public IQueryCommand UsingServices(IModelService modelService)
        {
            this.modelService = modelService;
            return this;
        }

        /// <inheritdoc cref="IUsingQueryServices.UsingServices(IModelService,IAudioService)"/>
        public IQueryCommand UsingServices(IModelService modelService, IAudioService audioService)
        {
            this.modelService = modelService;
            this.audioService = audioService;
            return this;
        }
        
        /// <inheritdoc cref="IUsingQueryServices.UsingServices(IModelService,IAudioService,IQueryMatchRegistry)"/>
        public IQueryCommand UsingServices(IModelService modelService, IAudioService audioService, IQueryMatchRegistry queryMatchRegistry)
        {
            this.modelService = modelService;
            this.audioService = audioService;
            this.queryMatchRegistry = queryMatchRegistry;
            return this;
        }

        /// <inheritdoc cref="IUsingQueryServices.UsingServices(IModelService,IVideoService)"/>
        public IQueryCommand UsingServices(IModelService modelService, IVideoService videoService)
        {
            this.modelService = modelService;
            this.videoService = videoService;
            return this;
        }

        /// <inheritdoc cref="IUsingQueryServices.UsingServices(IModelService,IVideoService,IQueryMatchRegistry)"/>
        public IQueryCommand UsingServices(IModelService modelService, IVideoService videoService, IQueryMatchRegistry queryMatchRegistry)
        {
            this.modelService = modelService;
            this.videoService = videoService;
            this.queryMatchRegistry = queryMatchRegistry;
            return this;
        }

        /// <inheritdoc cref="IUsingQueryServices.UsingServices(IModelService,IMediaService)"/>
        public IQueryCommand UsingServices(IModelService modelService, IMediaService mediaService)
        {
            this.modelService = modelService;
            this.mediaService = mediaService;
            return this;
        }

        /// <inheritdoc cref="IUsingQueryServices.UsingServices(IModelService,IMediaService,IQueryMatchRegistry)"/>
        public IQueryCommand UsingServices(IModelService modelService, IMediaService mediaService, IQueryMatchRegistry queryMatchRegistry)
        {
            this.modelService = modelService;
            this.mediaService = mediaService;
            this.queryMatchRegistry = queryMatchRegistry;
            return this;
        }

        /// <inheritdoc cref="IInterceptHashes.Intercept"/>
        public IUsingQueryServices Intercept(Func<AVHashes, AVHashes> hashes)
        {
            hashesInterceptor = hashes;
            return this;
        }

        /// <inheritdoc cref="IQueryCommand.Query()"/>
        public async Task<AVQueryResult> Query()
        {
            return await Query(DateTime.MinValue);
        }

        /// <inheritdoc cref="IQueryCommand.Query(DateTime)"/>
        public async Task<AVQueryResult> Query(DateTime relativeTo)
        {
            var usingFingerprintServices = createFingerprintCommand().WithFingerprintConfig(queryConfiguration.FingerprintConfiguration);
            var fingerprintCommand = SelectMediaServiceForFingerprintCommand(usingFingerprintServices);
            
            var hashes = await fingerprintCommand.Hash();
            var avHashes = relativeTo == DateTime.MinValue ? hashes : hashes.WithRelativeTo(relativeTo);
            
            var (audioHashes, videoHashes) = hashesInterceptor(avHashes);
            var avQueryResult = GetAvQueryResult(audioHashes, videoHashes, hashes.FingerprintingTime);
            
            if (avQueryResult.ContainsMatches && queryMatchRegistry != null)
            {
                string streamId = audioHashes?.StreamId ?? videoHashes?.StreamId ?? string.Empty;
                logger.LogDebug("AVQueryResult contains {Count} matches. Registering them with {QueryMatchRegistry} for stream {StreamId}", avQueryResult.ResultEntries.Count(), queryMatchRegistry, streamId);
                var avQueryMatches = avQueryResult.ResultEntries.Select(_ => _.ConvertToAvQueryMatch(streamId: streamId));
                queryMatchRegistry.RegisterMatches(avQueryMatches);
            }

            return avQueryResult;
        }

        private IFingerprintCommand SelectMediaServiceForFingerprintCommand(IUsingFingerprintServices usingFingerprintServices)
        {
            if (mediaService != null)
            {
                logger.LogDebug("Using media service {0} for query hashes generation", mediaService);
                return usingFingerprintServices.UsingServices(mediaService);
            }

            if (videoService != null)
            {
                logger.LogDebug("Using video service {0} for query hashes generation", mediaService);
                return usingFingerprintServices.UsingServices(videoService);
            }

            logger.LogDebug("Using audio service {0} for query hashes generation", audioService);
            return usingFingerprintServices.UsingServices(audioService);
        }

        private AVQueryResult GetAvQueryResult(Hashes? audioHashes, Hashes? videoHashes, AVFingerprintingTime avFingerprintingTime)
        {
            var audioResults = GetQueryResult(audioHashes, queryConfiguration.Audio);
            var videoResults = GetQueryResult(videoHashes, queryConfiguration.Video);
            var (audioMilliseconds, videoMilliseconds) = avFingerprintingTime;
            var queryCommandStats = new AVQueryCommandStats(audioResults?.CommandStats, videoResults?.CommandStats).WithFingerprintingDurationMilliseconds(audioMilliseconds, videoMilliseconds);
            return new AVQueryResult(audioResults, videoResults, new AVHashes(audioHashes, videoHashes, avFingerprintingTime), queryCommandStats);
        }

        private QueryResult? GetQueryResult(Hashes? hashes, QueryConfiguration configuration)
        {
            if (modelService == null)
            {
                throw new ArgumentException("Provide an instance of IModelService to query the storage via UsingServices(IModelService)", nameof(modelService));
            }
            
            return hashes != null ? queryFingerprintService.Query(hashes, configuration, modelService) : null;
        }
    }
}