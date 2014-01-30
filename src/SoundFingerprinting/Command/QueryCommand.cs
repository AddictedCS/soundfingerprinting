namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;

    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    internal sealed class QueryCommand : IQuerySource, IWithQueryAndFingerprintConfiguration, IQueryCommand
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private Func<IWithFingerprintConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintCommand> createFingerprintMethod;
        
        public QueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        public IFingerprintConfiguration FingerprintConfiguration { get; private set; }

        public IQueryConfiguration QueryConfiguration { get; private set; }

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

        public IWithQueryAndFingerprintConfiguration From(float[] audioSamples)
        {
            fingerprintingMethodFromSelector = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(audioSamples);
            return this;
        }

        public IQueryCommand WithConfigs(IFingerprintConfiguration fingerprintConfiguration, IQueryConfiguration configuration)
        {
            QueryConfiguration = configuration;
            FingerprintConfiguration = fingerprintConfiguration;
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithFingerprintConfig(fingerprintConfiguration);
            return this;
        }

        public IQueryCommand WithConfigs<T1, T2>() where T1 : IFingerprintConfiguration, new() where T2 : IQueryConfiguration, new()
        {
            QueryConfiguration = new T2();
            FingerprintConfiguration = new T1();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithFingerprintConfig(FingerprintConfiguration);
            return this;
        }

        public IQueryCommand WithConfigs(Action<CustomFingerprintConfiguration> fingerprintConfig, Action<CustomQueryConfiguration> queryConfig)
        {
            QueryConfiguration = new CustomQueryConfiguration();
            queryConfig((CustomQueryConfiguration)QueryConfiguration);
            FingerprintConfiguration = new CustomFingerprintConfiguration();
            fingerprintConfig((CustomFingerprintConfiguration)FingerprintConfiguration);
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithFingerprintConfig(FingerprintConfiguration);
            return this;
        }

        public IQueryCommand WithDefaultConfigs()
        {
            QueryConfiguration = new DefaultQueryConfiguration();
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithFingerprintConfig(FingerprintConfiguration);
            return this;
        }

        public Task<QueryResult> Query()
        {
            return createFingerprintMethod()
                                     .Hash()
                                     .ContinueWith(task => queryFingerprintService.Query(task.Result, QueryConfiguration), TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}