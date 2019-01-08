namespace SoundFingerprinting.Configuration
{
    using System;
    using SoundFingerprinting.Command;
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
            QueryConfiguration = new DefaultQueryConfiguration
            {
                ThresholdVotes = thresholdVotes,
                FingerprintConfiguration = new DefaultFingerprintConfiguration
                {
                    SpectrogramConfig = new DefaultSpectrogramConfig
                    {
                        Stride = stride
                    }
                }
            };
                
            ResultEntryFilter = resultEntryFilter;
            SuccessCallback = successCallback;
            PermittedGap = permittedGap;
            DidNotPassFilterCallback = didNotPassFilterCallback;
        }

        public int ThresholdVotes
        {
            get => QueryConfiguration.ThresholdVotes;
            set => QueryConfiguration.ThresholdVotes = value;
        }

        public IRealtimeResultEntryFilter ResultEntryFilter { get; set; }

        public Action<ResultEntry> SuccessCallback { get; set; }
        
        public Action<ResultEntry> DidNotPassFilterCallback { get; set; }

        public IStride Stride
        {
            get => QueryConfiguration.Stride;
            set => QueryConfiguration.Stride = value;
        }

        public double PermittedGap { get; set; }

        internal QueryConfiguration QueryConfiguration { get; }
    }
}