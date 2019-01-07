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
        
        private readonly IQueryFingerprintService queryFingerprintService = QueryFingerprintService.Instance;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = FingerprintCommandBuilder.Instance;
        
        private BlockingCollection<AudioSamples> realtimeSamples;
        private RealtimeQueryConfiguration realtimeQueryConfiguration;
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
            this.realtimeQueryConfiguration = realtimeQueryConfiguration;
            queryConfiguration = new DefaultQueryConfiguration
            {
                ThresholdVotes = realtimeQueryConfiguration.ThresholdVotes
            };
            return this;
        }

        public async Task Query(CancellationToken cancellationToken)
        {
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(realtimeQueryConfiguration.Stride, MinSamplesForOneFingerprint);
            var realtimeResultEntryAggregator = new StatefulRealtimeResultEntryAggregator();
            
            while (!realtimeSamples.IsAddingCompleted && !cancellationToken.IsCancellationRequested)
            {
                if (realtimeSamples.TryTake(out var audioSamples, Delay, cancellationToken))
                {
                    var prefixed = realtimeSamplesAggregator.Aggregate(audioSamples);
                    
                    var hashes = await fingerprintCommandBuilder.BuildFingerprintCommand()
                        .From(prefixed)
                        .UsingServices(audioService)
                        .Hash();

                    var results = queryFingerprintService.Query(hashes, queryConfiguration, modelService);

                    var realtimeQueryResult = realtimeResultEntryAggregator.Consume(results.ResultEntries, 
                        realtimeQueryConfiguration.ResultEntryFilter, 
                        audioSamples.Duration,
                        realtimeQueryConfiguration.PermittedGap);
                    
                    foreach (var result in realtimeQueryResult.SuccessEntries)
                    {
                        realtimeQueryConfiguration?.SuccessCallback(result);
                    }

                    foreach (var result in realtimeQueryResult.DidNotPassThresholdEntries)
                    {
                        realtimeQueryConfiguration?.DidNotPassFilterCallback(result);
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