namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Query;

    public interface IRealtimeSource
    {
        IWithRealtimeQueryConfiguration From(BlockingCollection<AudioSamples> audioSamples);
    }

    public interface IUsingRealtimeQueryServices
    {
        IRealtimeQueryCommand UsingServices(IModelService modelService);
    }
    
    public interface IWithRealtimeQueryConfiguration
    {
        IUsingRealtimeQueryServices WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration);
    }

    public class RealtimeQueryConfiguration
    {
        public RealtimeQueryConfiguration(int thresholdVotes, double confidenceThreshold, Action<ResultEntry> callback, TimeSpan approximateChunkLength)
        {
            ThresholdVotes = thresholdVotes;
            ConfidenceThreshold = confidenceThreshold;
            Callback = callback;
            ApproximateChunkLength = approximateChunkLength;
        }
        
        public int ThresholdVotes { get; }
        
        public double ConfidenceThreshold { get; }
        
        public TimeSpan ApproximateChunkLength { get; }

        public Action<ResultEntry> Callback { get; }
    }
}