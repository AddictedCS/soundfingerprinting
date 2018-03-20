namespace SoundFingerprinting.Command
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    public sealed class QueryCommand : IQuerySource, IWithQueryConfiguration, IQueryCommand
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private IModelService modelService;
        
        private Func<IWithFingerprintConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintCommand> createFingerprintMethod;

        private QueryConfiguration queryConfiguration;

        internal QueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            queryConfiguration = new DefaultQueryConfiguration();
        }

        public IWithQueryConfiguration From(string pathToAudioFile)
        {
            fingerprintingMethodFromSelector = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(pathToAudioFile);
            return this;
        }

        public IWithQueryConfiguration From(string pathToAudioFile, double secondsToProcess, double startAtSecond)
        {
            fingerprintingMethodFromSelector = () => fingerprintCommandBuilder.BuildFingerprintCommand()
                                                                              .From(pathToAudioFile, secondsToProcess, startAtSecond);
            return this;
        }

        public IWithQueryConfiguration From(AudioSamples audioSamples)
        {
            fingerprintingMethodFromSelector = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(audioSamples);
            return this;
        }

        public IUsingQueryServices WithQueryConfig(QueryConfiguration queryConfiguration)
        {
            this.queryConfiguration = queryConfiguration;
            return this;
        }

        public IUsingQueryServices WithQueryConfig(Func<QueryConfiguration, QueryConfiguration> amendQueryConfigFunctor)
        {
            queryConfiguration = amendQueryConfigFunctor(queryConfiguration);
            return this;
        }

        public IQueryCommand UsingServices(IModelService modelService, IAudioService audioService)
        {
            this.modelService = modelService;
            createFingerprintMethod = () => fingerprintingMethodFromSelector()
                                                .WithFingerprintConfig(queryConfiguration.FingerprintConfiguration)
                                                .UsingServices(audioService);
            return this;
        }

        public Task<QueryResult> Query()
        {
            var fingerprintingStopwatch = Stopwatch.StartNew();
            Task<QueryResult> resultingTask = createFingerprintMethod()
                                     .Hash()
                                     .ContinueWith(
                                        task =>
                                        {
                                            long fingerprintingTime = fingerprintingStopwatch.ElapsedMilliseconds;
                                            var hashes = task.Result;
                                            var queryStopwatch = Stopwatch.StartNew();
                                            QueryResult queryResult = queryFingerprintService.Query(hashes, queryConfiguration, modelService);
                                            long queryingTime = queryStopwatch.ElapsedMilliseconds;
                                            queryResult.Stats.FingerprintingDuration = fingerprintingTime;
                                            queryResult.Stats.QueryDuration = queryingTime;
                                            return queryResult;
                                        },
                                        TaskContinuationOptions.ExecuteSynchronously);

            return resultingTask;
        }
    }
}