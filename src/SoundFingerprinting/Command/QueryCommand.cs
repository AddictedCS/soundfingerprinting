namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public sealed class QueryCommand : IQuerySource, IWithQueryConfiguration, IQueryCommand
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private IModelService modelService;
        private IQueryMatchRegistry queryMatchRegistry;
        
        private Func<IWithFingerprintConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintCommand> createFingerprintCommand;

        private QueryConfiguration queryConfiguration;

        public QueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            queryConfiguration = new DefaultQueryConfiguration();
            queryMatchRegistry = NoOpQueryMatchRegistry.NoOp;
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

        public IWithQueryConfiguration From(Hashes hashedFingerprints)
        {
            createFingerprintCommand = () => new ExecutedFingerprintCommand(hashedFingerprints);
            return this;
        }

        public IUsingQueryServices WithQueryConfig(QueryConfiguration config)
        {
            queryConfiguration = config;
            return this;
        }

        public IUsingQueryServices WithQueryConfig(Func<QueryConfiguration, QueryConfiguration> amendQueryConfigFunctor)
        {
            queryConfiguration = amendQueryConfigFunctor(queryConfiguration);
            return this;
        }

        public IQueryCommand UsingServices(IModelService service, IAudioService audioService)
        {
            return UsingServices(service, audioService, NoOpQueryMatchRegistry.NoOp);
        }
        
        public IQueryCommand UsingServices(IModelService service, IAudioService audioService, IQueryMatchRegistry registry)
        {
            modelService = service;
            queryMatchRegistry = registry;
            
            if (createFingerprintCommand == null)
            {
                createFingerprintCommand = () => fingerprintingMethodFromSelector()
                    .WithFingerprintConfig(queryConfiguration.FingerprintConfiguration)
                    .UsingServices(audioService);
            }

            return this;
        }

        public async Task<QueryResult> Query()
        {
            return await Query(DateTime.Now);
        }

        public async Task<QueryResult> Query(DateTime relativeTo)
        {
            var fingerprintingStopwatch = Stopwatch.StartNew();
            var hashes = await createFingerprintCommand().Hash();
            long fingerprintingDuration = fingerprintingStopwatch.ElapsedMilliseconds;

            var queryStopwatch = Stopwatch.StartNew();
            var queryResult = queryFingerprintService.Query(hashes, queryConfiguration, relativeTo, modelService);
            long queryDuration = queryStopwatch.ElapsedMilliseconds;
            if (queryResult.ContainsMatches)
            {
                var queryMatches = queryResult.ResultEntries.ToQueryMatches();
                queryMatchRegistry.RegisterMatches(queryMatches, new Dictionary<string, string>());
            }

            return new QueryResult(queryResult.ResultEntries, new QueryStats(queryResult.Stats.TotalTracksAnalyzed,
                queryResult.Stats.TotalFingerprintsAnalyzed,
                queryDuration, fingerprintingDuration));
        }
    }
}