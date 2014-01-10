namespace SoundFingerprinting.Query
{
    using System;
    using System.Threading.Tasks;

    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query.Configuration;

    internal sealed class FingerprintingQueryCommand : IQuerySource, IWithQueryConfiguration, IWithQueryAndFingerprintConfiguration, IFingerprintQueryCommand
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private Func<IWithFingerprintConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintCommand> createFingerprintMethod;
        private IQueryConfiguration queryConfiguration;

        public FingerprintingQueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
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

        public IFingerprintQueryCommand WithConfigurations(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithAlgorithmConfiguration(fingerprintingConfiguration);
            return this;
        }

        public IFingerprintQueryCommand WithConfigurations<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : IQueryConfiguration, new()
        {
            queryConfiguration = new T2();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithAlgorithmConfiguration<T1>();
            return this;
        }

        public IFingerprintQueryCommand WithCustomConfigurations(
            Action<CustomFingerprintingConfiguration> fingerprintingConfigurationTransformation, Action<CustomQueryConfiguration> queryConfigurationTransformation)
        {
            CustomQueryConfiguration customQueryConfiguration = new CustomQueryConfiguration();
            queryConfiguration = customQueryConfiguration;
            queryConfigurationTransformation(customQueryConfiguration);
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithCustomAlgorithmConfiguration(fingerprintingConfigurationTransformation);
            return this;
        }

        public IFingerprintQueryCommand WithDefaultConfigurations()
        {
            queryConfiguration = new DefaultQueryConfiguration();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithDefaultAlgorithmConfiguration();
            return this;
        }

        public Task<QueryResult> Query()
        {
            return createFingerprintMethod()
                                     .Hash()
                                     .ContinueWith(task => queryFingerprintService.Query(task.Result, queryConfiguration), TaskContinuationOptions.ExecuteSynchronously);
        }

        public IFingerprintQueryCommand WithQueryConfiguration(IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            return this;
        }
    }
}