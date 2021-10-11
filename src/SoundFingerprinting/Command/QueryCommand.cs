namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Query command.
    /// </summary>
    public sealed class QueryCommand : IQuerySource, IWithQueryConfiguration 
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private IModelService modelService;
        private IAudioService audioService;
        private IQueryMatchRegistry queryMatchRegistry;
        
        private Func<IWithFingerprintConfiguration> createFingerprintCommand;

        private QueryConfiguration queryConfiguration;

        public QueryCommand(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
            queryConfiguration = new DefaultQueryConfiguration();
            queryMatchRegistry = NoOpQueryMatchRegistry.NoOp;
            this.audioService = new SoundFingerprintingAudioService();
        }

        /// <inheritdoc cref="IQuerySource.From(string)"/>
        public IWithQueryConfiguration From(string pathToAudioFile)
        {
            createFingerprintCommand = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(pathToAudioFile);
            return this;
        }

        /// <inheritdoc cref="IQuerySource.From(string,double,double)"/>
        public IWithQueryConfiguration From(string pathToAudioFile, double secondsToProcess, double startAtSecond)
        {
            createFingerprintCommand = () => fingerprintCommandBuilder.BuildFingerprintCommand()
                                                                      .From(pathToAudioFile, secondsToProcess, startAtSecond);
            return this;
        }

        /// <inheritdoc cref="IQuerySource.From(AudioSamples)"/>
        public IWithQueryConfiguration From(AudioSamples audioSamples)
        {
            createFingerprintCommand = () => fingerprintCommandBuilder.BuildFingerprintCommand().From(audioSamples);
            return this;
        }

        /// <inheritdoc cref="IQuerySource.From(Hashes)"/>
        public IWithQueryConfiguration From(Hashes hashes)
        {
            createFingerprintCommand = () => new ExecutedFingerprintCommand(hashes);
            return this;
        }

        /// <inheritdoc cref="IWithQueryConfiguration.WithQueryConfig(QueryConfiguration)"/>
        public IUsingQueryServices WithQueryConfig(QueryConfiguration config)
        {
            queryConfiguration = config;
            return this;
        }

        /// <inheritdoc cref="IWithQueryConfiguration.WithQueryConfig(Func{QueryConfiguration,QueryConfiguration})"/>
        public IUsingQueryServices WithQueryConfig(Func<QueryConfiguration, QueryConfiguration> amendQueryConfigFunctor)
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

        /// <inheritdoc cref="IQueryCommand.Query()"/>
        public async Task<QueryResult> Query()
        {
            return await Query(DateTime.MinValue);
        }

        /// <inheritdoc cref="IQueryCommand.Query(DateTime)"/>
        public async Task<QueryResult> Query(DateTime relativeTo)
        {
            var fingerprintingStopwatch = Stopwatch.StartNew();
            var hashes = await createFingerprintCommand()
                .WithFingerprintConfig(queryConfiguration.FingerprintConfiguration)
                .UsingServices(audioService)
                .Hash();
            
            long fingerprintingDuration = fingerprintingStopwatch.ElapsedMilliseconds;

            var queryHashes = relativeTo == DateTime.MinValue ? hashes : hashes.WithNewRelativeTo(relativeTo);
            var queryResult = queryFingerprintService.Query(queryHashes, queryConfiguration, modelService);
            if (queryResult.ContainsMatches)
            {
                var queryMatches = queryResult.ResultEntries.Select(_ => _.ToQueryMatch()).ToList();
                queryMatchRegistry.RegisterMatches(queryMatches, new Dictionary<string, string>());
            }

            return new QueryResult(queryResult.ResultEntries, hashes, queryResult.CommandStats.WithFingerprintingDurationMilliseconds(fingerprintingDuration));
        }
    }
}