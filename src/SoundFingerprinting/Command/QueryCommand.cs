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
        private IQueryConfiguration queryConfiguration;

        public QueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

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
            queryConfiguration = configuration;
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithFingerprintConfig(fingerprintConfiguration);
            return this;
        }

        public IQueryCommand WithConfigs<T1, T2>() where T1 : IFingerprintConfiguration, new() where T2 : IQueryConfiguration, new()
        {
            queryConfiguration = new T2();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithFingerprintConfig<T1>();
            return this;
        }

        public IQueryCommand WithConfigs(Action<CustomFingerprintConfiguration> fingerprintConfig, Action<CustomQueryConfiguration> queryConfig)
        {
            CustomQueryConfiguration customQueryConfiguration = new CustomQueryConfiguration();
            queryConfiguration = customQueryConfiguration;
            queryConfig(customQueryConfiguration);
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithFingerprintConfig(fingerprintConfig);
            return this;
        }

        public IQueryCommand WithDefaultConfigs()
        {
            queryConfiguration = new DefaultQueryConfiguration();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithDefaultFingerprintConfig();
            return this;
        }

        public Task<QueryResult> Query()
        {
            return createFingerprintMethod()
                                     .Hash()
                                     .ContinueWith(task => queryFingerprintService.Query(task.Result, queryConfiguration), TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}