namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

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
        public RealtimeQueryConfiguration(int thresholdVotes, 
            double secondsThreshold, 
            Action<ResultEntry> callback, 
            TimeSpan approximateChunkLength, 
            IStride stride)
        {
            ThresholdVotes = thresholdVotes;
            SecondsThreshold = secondsThreshold;
            Callback = callback;
            ApproximateChunkLength = approximateChunkLength;
            Stride = stride;
        }
        
        public int ThresholdVotes { get; }
        
        public double SecondsThreshold { get; }
        
        public TimeSpan ApproximateChunkLength { get; }

        public Action<ResultEntry> Callback { get; }
        
        public IStride Stride { get; }
    }
}