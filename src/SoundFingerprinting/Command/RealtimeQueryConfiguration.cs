namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    public class RealtimeQueryConfiguration
    {
        public RealtimeQueryConfiguration(int thresholdVotes,
            IRealtimeResultEntryFilter resultEntryFilter,
            Action<ResultEntry> successCallback,
            Action<ResultEntry> didNotPassFilterCallback,
            IStride stride,
            double permittedGap)
        {
            ThresholdVotes = thresholdVotes;
            ResultEntryFilter = resultEntryFilter;
            SuccessCallback = successCallback;
            Stride = stride;
            PermittedGap = permittedGap;
            DidNotPassFilterCallback = didNotPassFilterCallback;
        }
        
        public int ThresholdVotes { get; }
        
        public IRealtimeResultEntryFilter ResultEntryFilter { get; }

        public Action<ResultEntry> SuccessCallback { get; }
        
        public Action<ResultEntry> DidNotPassFilterCallback { get; }
        
        public IStride Stride { get; }
        
        public double PermittedGap { get; }
    }
}