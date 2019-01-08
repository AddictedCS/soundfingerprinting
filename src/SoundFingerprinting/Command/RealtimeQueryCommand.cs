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
        private QueryConfiguration queryConfiguration;
        private IModelService modelService;
        private IAudioService audioService;
        
        public IWithRealtimeQueryConfiguration From(BlockingCollection<AudioSamples> audioSamples)
        {
            realtimeSamples = audioSamples;
            return this;
        }

        public IUsingRealtimeQueryServices WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration)
        {
            this.configuration = realtimeQueryConfiguration;
            queryConfiguration = new DefaultQueryConfiguration
            {
                ThresholdVotes = realtimeQueryConfiguration.ThresholdVotes
            };
            return this;
        }

        public async Task Query(CancellationToken cancellationToken)
        {
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(configuration.Stride, MinSamplesForOneFingerprint);
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(configuration.ResultEntryFilter, configuration.PermittedGap);
            
            while (!realtimeSamples.IsAddingCompleted && !cancellationToken.IsCancellationRequested)
            {
                if (realtimeSamples.TryTake(out var audioSamples, Delay, cancellationToken))
                {
                    var prefixed = realtimeSamplesAggregator.Aggregate(audioSamples);
                    
                    var hashes = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                        .From(prefixed)
                        .UsingServices(audioService)
                        .Hash();

                    var results = QueryFingerprintService.Instance.Query(hashes, queryConfiguration, modelService);

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