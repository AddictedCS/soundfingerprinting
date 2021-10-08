namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    /// <summary>
    ///   Configuration options used when querying the storage.
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
            get => thresholdVotes;

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
        ///  Gets or sets maximum number of tracks to return out of all analyzed candidates.
        /// </summary>
        public int MaxTracksToReturn
        {
            get => maxTracksToReturn;

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
        ///  Gets or sets stride between 2 consecutive fingerprints used during querying.
        /// </summary>
        public IStride Stride
        {
            get => FingerprintConfiguration.SpectrogramConfig.Stride;

            set => FingerprintConfiguration.SpectrogramConfig.Stride = value;
        }

        /// <summary>
        ///  Gets or sets the number of top wavelets to analyze.
        /// </summary>
        public int TopWavelets
        {
            get => FingerprintConfiguration.TopWavelets;

            set => FingerprintConfiguration.TopWavelets = value;
        }

        /// <summary>
        ///  Gets or sets frequency range to analyze when creating the fingerprint.
        /// </summary>
        public FrequencyRange FrequencyRange
        {
            get => FingerprintConfiguration.FrequencyRange;

            set => FingerprintConfiguration.FrequencyRange = value;
        }

        /// <summary>
        ///  Gets or sets a value indicating whether the algorithm should search for multiple matches of the same track in the query. 
        ///  Useful when you have a long query which may contain same track multiple times scattered across the query.
        ///  Use cautiously, since aligning same track on a long query multiple times may result in a performance penalty. Default is false.
        /// </summary>
        public bool AllowMultipleMatchesOfTheSameTrackInQuery { get; set; }

        /// <summary>
        ///  Gets or sets permitted gap between consecutive matches of the same track.
        ///  A gap indicates the difference between query and target track, permitted gap defines the length of the gap to be ignored.
        /// </summary>
        public double PermittedGap { get; set; }

        /// <summary>
        ///  Gets or sets meta fields filters that are passed to the storage when querying. Useful for second stage filtering.
        ///  Once set, in order for the track to be considered as an eligible candidate it <b>MUST</b> contain same meta-fields (see <see cref="TrackInfo.MetaFields"/>). <br/>
        ///  Example: TrackInfo has meta field "Region: USA". You can specify "Region: USA".
        /// </summary>
        public IDictionary<string, string> YesMetaFieldsFilters { get; set; } = null!;

        /// <summary>
        ///  Gets or sets meta fields filters that are passed to the storage when querying. Useful for second stage filtering.
        ///  Once set, in order for the track to be considered as an eligible candidate it <b>MUST NOT</b> contain same meta-fields (see <see cref="TrackInfo.MetaFields"/>). <br/>
        ///  Example: TrackInfo has meta field "Region: USA". You can specify "Region: USA" in order to remove all matches from USA.
        /// </summary>
        public IDictionary<string, string> NoMetaFieldsFilters { get; set; } = null!;

        /// <summary>
        ///  Gets or sets query media type.
        ///  Source of fingerprints is either audio or video, set the corresponding type so that the ModelService is aware where to look for matches.
        /// </summary>
        public MediaType QueryMediaType { get; set; }

        /// <summary>
        ///  Gets or sets fingerprint configuration used during querying.
        /// </summary>
        /// <remarks>
        ///   In order to get successful matches, same fingerprinting configuration has be used during fingerprinting generation and query.
        /// </remarks>
        public FingerprintConfiguration FingerprintConfiguration { get; set; } = null!;
    }
}