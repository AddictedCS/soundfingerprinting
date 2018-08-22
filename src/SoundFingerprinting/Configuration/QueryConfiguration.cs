namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Strides;

    /// <summary>
    ///   Configuration options used when querying the ModelService
    /// </summary>
    public abstract class QueryConfiguration
    {
        private int thresholdVotes;
        private int maxTracksToReturn;

        /// <summary>
        ///   Gets or sets vote count for a track to be considered a potential match (i.e. [1; 25]).
        /// </summary>
        public int ThresholdVotes
        {
            get
            {
                return thresholdVotes;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("ThresholdVotes cannot be less than 1", nameof(value));
                }

                thresholdVotes = value;
            }
        }

        /// <summary>
        ///  Gets or sets maximum number of tracks to return out of all analyzed candidates
        /// </summary>
        public int MaxTracksToReturn
        {
            get
            {
                return maxTracksToReturn;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("MaxTracksToReturn cannot be less or equal to 0", nameof(value));
                }

                maxTracksToReturn = value;
            }
        }

        /// <summary>
        ///  Gets or sets stride between 2 consecutive fingerprints used during querying
        /// </summary>
        public IStride Stride
        {
            get
            {
                return FingerprintConfiguration.SpectrogramConfig.Stride;
            }

            set
            {
                FingerprintConfiguration.SpectrogramConfig.Stride = value;
            }
        }

        /// <summary>
        ///   Gets or sets Haar Wavelet norm.
        /// </summary>
        public double HaarWaveletNorm
        {
            get
            {
                return FingerprintConfiguration.HaarWaveletNorm;
            }

            set
            {
                FingerprintConfiguration.HaarWaveletNorm = value;
            }
        }

        /// <summary>
        ///  Gets or sets the scaling function used in spectral image creation
        /// </summary>
        public Func<float, float, float> ScalingFunction
        {
            get
            {
                return FingerprintConfiguration.ScalingFunction;
            }

            set
            {
                FingerprintConfiguration.ScalingFunction = value;
            }
        }

        /// <summary>
        ///  Gets or sets the number of top wavelets to analyze
        /// </summary>
        public int TopWavelets
        {
            get
            {
                return FingerprintConfiguration.TopWavelets;
            }

            set
            {
                FingerprintConfiguration.TopWavelets = value;
            }
        }

        /// <summary>
        ///  Gets or sets frequency range to analyze when creating the fingerprint
        /// </summary>
        public FrequencyRange FrequencyRange
        {
            get
            {
                return FingerprintConfiguration.FrequencyRange;
            }

            set
            {
                FingerprintConfiguration.FrequencyRange = value;
            }
        }

        /// <summary>
        ///  Gets or sets list of clusters to consider when querying the datasource for potential candidates
        /// </summary>
        public IEnumerable<string> Clusters { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether the algorithm should search for multiple matches of the same track in the query. 
        ///  Useful when you have a long query which may contain same track multiple times scatered across the query.
        ///  Use cautiously, since aligning same track on a long query multiple times may result in a performance penalty. Default is false.
        /// </summary>
        public bool AllowMultipleMatchesOfTheSameTrackInQuery { get; set; }

        /// <summary>
        ///  Gets or sets meta fields that are passed to the storage when querying. Useful for statistics gathering and such.
        /// </summary>
        public IDictionary<string, string> MetaFields { get; set; }

        /// <summary>
        ///  Gets or sets fingerprint configuration used during querying. This field will be used later on for internal purposes. 
        /// </summary>
        internal FingerprintConfiguration FingerprintConfiguration { get; set; }
    }
}