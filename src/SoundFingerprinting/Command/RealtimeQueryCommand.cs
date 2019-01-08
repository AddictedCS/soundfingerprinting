namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    public interface IRealtimeQueryCommand
    {
        Task Query(CancellationToken cancellationToken);
    }
    
    public class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration, IUsingRealtimeQueryServices, IRealtimeQueryCommand
    {
        private const int MinSamplesForOneFingerprint = 10240;
        private const int Delay = MinSamplesForOneFingerprint / 5512;

        private BlockingCollection<AudioSamples> realtimeSamples;
        private RealtimeQueryConfiguration configuration;
        private IModelService modelService;
        private IAudioService audioService;

        public RealtimeQueryCommand()
        {
            configuration = new DefaultRealtimeQueryConfiguration(e => throw new Exception("Register a success callback for your realtime query"), e => { /* do nothing */ });
        }
        
        public IWithRealtimeQueryConfiguration From(BlockingCollection<AudioSamples> audioSamples)
        {
            realtimeSamples = audioSamples;
            return this;
        }

        public IUsingRealtimeQueryServices WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration)
        {
            configuration = realtimeQueryConfiguration;
            return this;
        }

        public IUsingRealtimeQueryServices WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor)
        {
            configuration = amendQueryFunctor(configuration);
            return this;
        }

        public async Task Query(CancellationToken cancellationToken)
        {
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(configuration.Stride, MinSamplesForOneFingerprint);
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(configuration.ResultEntryFilter, configuration.PermittedGap);
            var commandBuilder = FingerprintCommandBuilder.Instance;
            var queryFingerprintService = QueryFingerprintService.Instance;
            
            while (!realtimeSamples.IsAddingCompleted && !cancellationToken.IsCancellationRequested)
            {
                if (realtimeSamples.TryTake(out var audioSamples, Delay, cancellationToken))
                {
                    var prefixed = realtimeSamplesAggregator.Aggregate(audioSamples);

                    var hashes = await commandBuilder.BuildFingerprintCommand()
                        .From(prefixed)
                        .WithFingerprintConfig(configuration.QueryConfiguration.FingerprintConfiguration)
                        .UsingServices(audioService)
                        .Hash();

                    var results = queryFingerprintService.Query(hashes, configuration.QueryConfiguration, modelService);

                    var realtimeQueryResult = resultsAggregator.Consume(results.ResultEntries, audioSamples.Duration);
                    
                    foreach (var result in realtimeQueryResult.SuccessEntries)
                    {
                        configuration?.SuccessCallback(result);
                    }

                    foreach (var result in realtimeQueryResult.DidNotPassThresholdEntries)
                    {
                        configuration?.DidNotPassFilterCallback(result);
                    }
                }
            }
        }

        public IRealtimeQueryCommand UsingServices(IModelService modelService)
        {
            this.modelService = modelService;
            this.audioService = new SoundFingerprintingAudioService();
            return this;
        }
    }
}