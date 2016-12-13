namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;

    /// <summary>
    ///  Class containing useful information about the result entry match
    /// </summary>
    public class ResultEntry
    {
        internal ResultEntry(TrackData track, double queryMatchStartsAt, double queryMatchLength, double originStartsAt, double trackStartsAt, double confidence, int hammingSimilaritySum, double queryLength, MatchedPair bestMatch)
        {
            Track = track;
            QueryMatchStartsAt = queryMatchStartsAt;
            QueryMatchLength = queryMatchLength;
            TrackMatchStartsAt = originStartsAt;
            Confidence = confidence;
            HammingSimilaritySum = hammingSimilaritySum;
            TrackStartsAt = trackStartsAt;
            QueryLength = queryLength;
            BestMatch = bestMatch;
        }

        /// <summary>
        ///  Gets the resulting matched track from the datastore
        /// </summary>
        public TrackData Track { get; private set; }
        
        /// <summary>
        ///  Gets the best guess of how long in seconds did the resulting track matched in the query
        /// </summary>
        public double QueryMatchLength { get; private set; }
        
        /// <summary>
        ///  Gets the exact position in seconds where resulting track started to match in the query
        /// </summary>
        /// <example>
        ///  Query length is of 30 seconds. It started to match at 10th second, <code>QueryMatchStartsAt</code> will be equal to 10.
        /// </example>
        public double QueryMatchStartsAt { get; private set; }

        /// <summary>
        ///  Gets best guess in seconds where does the result track starts in the query snippet. This value may be negative.
        /// </summary>
        /// <example>
        ///   Resulting Track <c>A</c> in the datastore is of 30 sec. The query is of 10 seconds, with <code>TrackMatchStartsAt</code> at 15th second. <code>TrackStartsAt</code> will be equal to -15;
        /// </example>
        public double TrackStartsAt { get; private set; }

        /// <summary>
        ///  Gets the time position in seconds where the origin track started to match the query
        /// </summary>
        /// <example>
        ///  Resulting track <c>A</c> in the datastore is of 100 sec. The query started to match at 40th sec. <code>TrackMatchStartsAt</code> will be equal to 40.
        /// </example>
        public double TrackMatchStartsAt { get; private set; }

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
        ///  Gets the value [0, 1) of how confident is the framework that query match corresponds to result track
        /// </summary>
        public double Confidence { get; private set; }

        /// <summary>
        ///  Gets best matched pair
        /// </summary>
        internal MatchedPair BestMatch { get; private set; }

                /// <summary>
        ///  Gets similarity count between query match and track
        /// </summary>
        internal int HammingSimilaritySum { get; private set; }

        /// <summary>
        ///  Gets the exact query length used to generate this entry
        /// </summary>
        internal double QueryLength { get; private set; }
    }
}