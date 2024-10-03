namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    /// <summary>
    ///   Configuration options used when querying the storage.
    /// </summary>
    public abstract class QueryConfiguration
    {
        private int thresholdVotes;
        private int maxTracksToReturn;
        private QueryPathReconstructionStrategyType queryPathReconstructionStrategyType;

        /// <summary>
        ///   Gets or sets vote count for a track to be considered a potential match (i.e. [1; 25]).
        /// </summary>
        /// <remarks>
        ///  Each fingerprints contains a predefined number of integers that describe it <see cref="HashedFingerprint.HashBins"/> (by default 25 integers).
        ///  Threshold votes control how many of those integers have to match to report a successful match.
        ///  The higher the number the more precise the content from the query and track have to be (default = 4).
        /// </remarks>
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
        ///  Gets or sets maximum number of tracks to return out of all the analyzed candidates.
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
        /// <remarks>
        ///  You don't have to use the same stride when inserting and querying.
        ///  It is better to use a randomized query stride to minimize the probability of unlucky alignments between query and target track.
        ///  Default is <see cref="IncrementalRandomStride"/> with 256-512 range.
        /// </remarks>
        public IStride Stride
        {
            get => FingerprintConfiguration.Stride;
            set => FingerprintConfiguration.Stride = value;
        }

        /// <inheritdoc cref="SoundFingerprinting.Configuration.FingerprintConfiguration.TopWavelets"/>
        public int TopWavelets
        {
            get => FingerprintConfiguration.TopWavelets;
            set => FingerprintConfiguration.TopWavelets = value;
        }

        /// <inheritdoc cref="SoundFingerprinting.Configuration.FingerprintConfiguration.FrequencyRange"/>
        public FrequencyRange FrequencyRange
        {
            get => FingerprintConfiguration.FrequencyRange;
            set => FingerprintConfiguration.FrequencyRange = value;
        }

        /// <summary>
        ///  Gets or sets an enum value instructing the algorithm to reconstruct query path strategy according to specified strategy
        /// </summary>
        /// <remarks>
        ///  Default for audio is <see cref="QueryPathReconstructionStrategyType.MultipleBestPaths"/>. <br/>
        ///  Default for video is <see cref="QueryPathReconstructionStrategyType.Legacy"/>. <br />
        ///  As <b>AllowMultipleMatchesOfTheSameTrackInQuery</b> was removed from the API (v8.17.0), it got substituted by <see cref="QueryPathReconstructionStrategyType.MultipleBestPaths"/>, making the implementations equivalent.
        /// </remarks>
        public QueryPathReconstructionStrategyType QueryPathReconstructionStrategy
        {
            get => queryPathReconstructionStrategyType;
            set
            {
                ScoreAlgorithm = value == QueryPathReconstructionStrategyType.Legacy ? HammingSimilarityScoreAlgorithm.Instance : SubFingerprintCountScoreAlgorithm.Instance;
                queryPathReconstructionStrategyType = value;
            }
        }

        /// <summary>
        ///  Gets or sets permitted gap between consecutive matches of the same track (as defined by the <see cref="Coverage.BestPath"/> property).
        ///  A gap indicates the difference between query and target track, permitted gap defines the length of the gap to be ignored.
        /// </summary>
        /// <remarks>
        ///  <see cref="Coverage" /> describes in detail when the match occurred, including any gaps that happened along the way.
        ///  Permitted gap instructs the algorithm to ignore gaps of a certain length.
        /// </remarks>
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
        ///  Gets or sets fingerprint configuration used during querying.
        /// </summary>
        /// <remarks>
        ///   In order to get successful matches, same fingerprinting configuration has be used during fingerprinting generation and query.
        /// </remarks>
        public FingerprintConfiguration FingerprintConfiguration { get; set; } = null!;
        
        /// <summary>
        ///  Gets scoring algorithm, used to calculate similarity between track/query matches.
        /// </summary>
        /// <remarks>
        ///  Before v8.16.2 hamming similarity was used to measure how similar query/track pairs are.
        ///  Since hamming similarity is not a good similarity metric (specifically when applied to hashed min-hashes), scoring algorithm was reduced to sub-fingerprints counting (since score is 1 for all pairs <see cref="SubFingerprintCountScoreAlgorithm"/>).
        /// </remarks>
        public IScoreAlgorithm ScoreAlgorithm { get; private set; } = null!;
    }
}