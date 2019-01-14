namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    public class RealtimeQueryConfiguration
    {
        public RealtimeQueryConfiguration(int thresholdVotes,
            IRealtimeResultEntryFilter resultEntryFilter,
            Action<ResultEntry> successCallback,
            Action<ResultEntry> didNotPassFilterCallback,
            Action<List<HashedFingerprint>> queryFingerprintsCallback,
            IStride stride,
            double permittedGap,
            IEnumerable<string> clusters)
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
                },
                Clusters = clusters
            };
                
            ResultEntryFilter = resultEntryFilter;
            SuccessCallback = successCallback;
            PermittedGap = permittedGap;
            DidNotPassFilterCallback = didNotPassFilterCallback;
            QueryFingerprintsCallback = queryFingerprintsCallback;
        }

        public int ThresholdVotes
        {
            get => QueryConfiguration.ThresholdVotes;
            set => QueryConfiguration.ThresholdVotes = value;
        }

        public IRealtimeResultEntryFilter ResultEntryFilter { get; set; }

        public Action<ResultEntry> SuccessCallback { get; set; }
        
        public Action<ResultEntry> DidNotPassFilterCallback { get; set; }

        public Action<List<HashedFingerprint>> QueryFingerprintsCallback { get; set; }

        public IStride Stride
        {
            get => QueryConfiguration.Stride;
            set => QueryConfiguration.Stride = value;
        }

        public double PermittedGap { get; set; }

        public IEnumerable<string> Clusters
        {
            get => QueryConfiguration.Clusters;
            set => QueryConfiguration.Clusters = value;
        }

        internal QueryConfiguration QueryConfiguration { get; }
    }
}