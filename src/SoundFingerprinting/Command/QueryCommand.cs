namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

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
            FingerprintConfiguration = new EfficientFingerprintConfigurationForQuerying();
            QueryConfiguration = new DefaultQueryConfiguration { FingerprintConfiguration = FingerprintConfiguration };
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

        public IUsingQueryServices WithFingerprintConfig(FingerprintConfiguration fingerprintConfiguration)
        {
            FingerprintConfiguration = fingerprintConfiguration;
            return this;
        }

        public IUsingQueryServices WithFingerprintConfig(Action<FingerprintConfiguration> amendFingerprintConfigFunctor)
        {
            amendFingerprintConfigFunctor(FingerprintConfiguration);
            return this;
        }

        public IUsingQueryServices WithQueryConfig(QueryConfiguration queryConfiguration)
        {
            QueryConfiguration = queryConfiguration;
            return this;
        }

        public IUsingQueryServices WithQueryConfig(Action<QueryConfiguration> amendQueryConfigFunctor)
        {
            amendQueryConfigFunctor(QueryConfiguration);
            return this;
        }

        public IUsingQueryServices WithConfigs(FingerprintConfiguration fingerprintConfiguration, QueryConfiguration configuration)
        {
            QueryConfiguration = configuration;
            FingerprintConfiguration = fingerprintConfiguration;
            return this;
        }

        public IUsingQueryServices WithConfigs(Action<FingerprintConfiguration> amendFingerprintFunctor, Action<QueryConfiguration> amendQueryConfigFunctor)
        {
            amendQueryConfigFunctor(QueryConfiguration);
            amendFingerprintFunctor(FingerprintConfiguration);
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
            QueryConfiguration.FingerprintConfiguration = FingerprintConfiguration;
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

        public Task<QueryResult> QueryExperimental()
        {
            QueryConfiguration.FingerprintConfiguration = FingerprintConfiguration;
            return createFingerprintMethod()
                                     .Hash()
                                     .ContinueWith(
                                        task =>
                                        {
                                            var hashes = task.Result;
                                            return queryFingerprintService.QueryExperimental(modelService, hashes, QueryConfiguration);
                                        },
                                        TaskContinuationOptions.ExecuteSynchronously);
        }


        public Task<QueryResult> QueryWithTimeSequenceInformation()
        {
            QueryConfiguration.FingerprintConfiguration = FingerprintConfiguration;
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