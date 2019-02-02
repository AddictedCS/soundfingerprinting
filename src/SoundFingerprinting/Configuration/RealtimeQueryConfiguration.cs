namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    /// <summary>
    ///   Configuration options used when querying the data source in realtime
    /// </summary>
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
                Clusters = clusters,
                PermittedGap = permittedGap
            };
                
            ResultEntryFilter = resultEntryFilter;
            SuccessCallback = successCallback;
            DidNotPassFilterCallback = didNotPassFilterCallback;
            QueryFingerprintsCallback = queryFingerprintsCallback;
        }

        /// <summary>
        ///   Gets or sets vote count for a track to be considered a potential match (i.e. [1; 25]).
        /// </summary>
        public int ThresholdVotes
        {
            get => QueryConfiguration.ThresholdVotes;
            set => QueryConfiguration.ThresholdVotes = value;
        }

        /// <summary>
        ///  Gets or sets result entry filter
        /// </summary>
        public IRealtimeResultEntryFilter ResultEntryFilter { get; set; }

        /// <summary>
        ///   Gets or sets success callback invoked when a candidate passes result entry filter
        /// </summary>
        public Action<ResultEntry> SuccessCallback { get; set; }
        
        /// <summary>
        ///  Gets or sets callback invoked when a candidate did not pass result entry filter, but has been considered a candidate
        /// </summary>
        public Action<ResultEntry> DidNotPassFilterCallback { get; set; }

        /// <summary>
        ///  Gets or sets fingerprints callback which allows to intercept fingerprints used during querying
        /// </summary>
        public Action<List<HashedFingerprint>> QueryFingerprintsCallback { get; set; }

        /// <summary>
        ///  Gets or sets stride between 2 consecutive fingerprints used during querying
        /// </summary>
        public IStride Stride
        {
            get => QueryConfiguration.Stride;
            set => QueryConfiguration.Stride = value;
        }

        /// <summary>
        ///  Gets or sets permitted gap between consecutive candidate so that they are glued together
        /// </summary>
        public double PermittedGap
        {
            get => QueryConfiguration.PermittedGap;
            set => QueryConfiguration.PermittedGap = value;
        }

        /// <summary>
        ///  Gets or sets list of clusters to consider when querying the data source for potential candidates
        /// </summary>
        public IEnumerable<string> Clusters
        {
            get => QueryConfiguration.Clusters;
            set => QueryConfiguration.Clusters = value;
        }

        internal QueryConfiguration QueryConfiguration { get; }
    }
}