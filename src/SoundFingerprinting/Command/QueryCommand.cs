namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    internal sealed class QueryCommand : IQuerySource, IWithQueryAndFingerprintConfiguration, IQueryCommand
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private IModelService modelService;
        
        private Func<IWithFingerprintConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintCommand> createFingerprintMethod;

        public QueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
            QueryConfiguration = new DefaultQueryConfiguration();
        }

        public FingerprintConfiguration FingerprintConfiguration { get; private set; }

        public QueryConfiguration QueryConfiguration { get; private set; }

        public IWithQueryAndFingerprintConfiguration From(string pathToAudioFile)
        {
            fingerprintingMethodFromSelector = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(pathToAudioFile);
            return this;
        }

        public IWithQueryAndFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            fingerprintingMethodFromSelector = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(pathToAudioFile, secondsToProcess, startAtSecond);
            return this;
        }

        public IWithQueryAndFingerprintConfiguration From(AudioSamples audioSamples)
        {
            fingerprintingMethodFromSelector = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(audioSamples);
            return this;
        }

        public IUsingQueryServices WithConfigs(FingerprintConfiguration fingerprintConfiguration, QueryConfiguration configuration)
        {
            QueryConfiguration = configuration;
            FingerprintConfiguration = fingerprintConfiguration;
            return this;
        }

        public IUsingQueryServices WithConfigs<T1, T2>() where T1 : FingerprintConfiguration, new() where T2 : QueryConfiguration, new()
        {
            QueryConfiguration = new T2();
            FingerprintConfiguration = new T1();
            return this;
        }

        public IUsingQueryServices WithConfigs(Action<CustomFingerprintConfiguration> fingerprintConfig, Action<CustomQueryConfiguration> queryConfig)
        {
            QueryConfiguration = new CustomQueryConfiguration();
            queryConfig((CustomQueryConfiguration)QueryConfiguration);
            FingerprintConfiguration = new CustomFingerprintConfiguration();
            fingerprintConfig((CustomFingerprintConfiguration)FingerprintConfiguration);
            return this;
        }

        public IQueryCommand UsingServices(IModelService modelService, IAudioService audioService)
        {
            this.modelService = modelService;
            createFingerprintMethod = () => fingerprintingMethodFromSelector()
                                                .WithFingerprintConfig(FingerprintConfiguration)
                                                .UsingServices(audioService);
            return this;
        }

        public Task<QueryResult> Query()
        {
            return createFingerprintMethod()
                                     .Hash()
                                     .ContinueWith(
                                        task =>
                                            {
                                                var hashes = task.Result;
                                                return queryFingerprintService.Query(modelService, hashes, QueryConfiguration);
                                            },
                                        TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task<QueryResult> QueryWithTimeSequenceInformation()
        {
            return createFingerprintMethod()
                                    .Hash()
                                    .ContinueWith(
                                       task =>
                                       {
                                           var hashes = task.Result;
                                           return queryFingerprintService.QueryWithTimeSequenceInformation(modelService, hashes, QueryConfiguration);
                                       },
                                       TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}