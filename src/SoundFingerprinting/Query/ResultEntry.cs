namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;

    /// <summary>
    ///  Data class containing information about the resulting track
    /// </summary>
    public class ResultEntry
    {
        internal ResultEntry(TrackData track, double queryMatchStartsAt, double queryMatchLength, double queryCoverageLength, double originStartsAt, double trackStartsAt, double confidence, int hammingSimilaritySum, double queryLength)
        {
            Track = track;
            QueryMatchStartsAt = queryMatchStartsAt;
            QueryMatchLength = queryMatchLength;
            QueryCoverageLength = queryCoverageLength;
            TrackMatchStartsAt = originStartsAt;
            Confidence = confidence;
            HammingSimilaritySum = hammingSimilaritySum;
            TrackStartsAt = trackStartsAt;
            QueryLength = queryLength;
        }

        /// <summary>
        ///  Gets the resulting matched track from the datastore
        /// </summary>
        public TrackData Track { get; }
        
        /// <summary>
        ///  Gets the exact length of matched query within the target track.
        /// </summary>
        public double QueryMatchLength { get; }
        
        /// <summary>
        ///  Gets the exact position in seconds where resulting track started to match in the query
        /// </summary>
        /// <example>
        ///  Query length is of 30 seconds. It started to match at 10th second, <code>QueryMatchStartsAt</code> will be equal to 10.
        /// </example>
        public double QueryMatchStartsAt { get; }

        /// <summary>
        ///  Gets best guess in seconds where does the result track starts in the query snippet. This value may be negative.
        /// </summary>
        /// <example>
        ///   Resulting Track <c>A</c> in the datastore is of 30 sec. The query is of 10 seconds, with <code>TrackMatchStartsAt</code> at 15th second. <code>TrackStartsAt</code> will be equal to -15;
        /// </example>
        public double TrackStartsAt { get; }

        /// <summary>
        ///  Gets the time position in seconds where the origin track started to match the query
        /// </summary>
        /// <example>
        ///  Resulting track <c>A</c> in the datastore is of 100 sec. The query started to match at 40th sec. <code>TrackMatchStartsAt</code> will be equal to 40.
        /// </example>
        public double TrackMatchStartsAt { get; }

        /// <summary>
        ///  Gets the percentange of how much the query match covered the original track
        /// </summary>
        public double Coverage
        {
            get
            {
                return QueryMatchLength / Track.Length;
            }
        }

        /// <summary>
        ///  Gets the estimated percentage of how much the resulting track got covered by the query
        /// </summary>
        public double EstimatedCoverage
        {
            get
            {
                return QueryCoverageLength / Track.Length;
            }
        }

        /// <summary>
        ///  Gets the value [0, 1) of how confident is the framework that query match corresponds to result track
        /// </summary>
        public double Confidence { get; }

        /// <summary>
        ///  Gets similarity count between query match and track
        /// </summary>
        internal int HammingSimilaritySum { get; }

        /// <summary>
        ///  Gets the exact query length used to generate this entry
        /// </summary>
        internal double QueryLength { get; }

        /// <summary>
        ///  Gets estimated track coverage infered from matching start and end of the resulting track in the query
        /// </summary>
        internal double QueryCoverageLength { get; set; }
    }
}