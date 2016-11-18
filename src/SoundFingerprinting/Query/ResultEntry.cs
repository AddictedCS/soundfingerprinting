namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;

    public class ResultEntry
    {
        internal ResultEntry(TrackData track, double queryMatchStartsAt, double queryMatchLength, double originStartsAt, double trackStartsAt, double confidence, int hammingSimilaritySum, double queryLength, MatchedPair bestMatch)
        {
            Track = track;
            QueryMatchStartsAt = queryMatchStartsAt;
            QueryMatchLength = queryMatchLength;
            OriginStartsAt = originStartsAt;
            Confidence = confidence;
            HammingSimilaritySum = hammingSimilaritySum;
            TrackStartsAt = trackStartsAt;
            QueryLength = queryLength;
            BestMatch = bestMatch;
        }

        /// <summary>
        ///  Gets the resulting track
        /// </summary>
        public TrackData Track { get; private set; }
        
        /// <summary>
        ///  Gets the best guess of how long did the resulting track matched in the query
        /// </summary>
        public double QueryMatchLength { get; private set; }

        /// <summary>
        ///  Gets best guess where does the result track starts in the query snippet.
        /// </summary>
        public double TrackStartsAt { get; private set; }

        /// <summary>
        ///  Gets the percentange of how much the query match covered the original track
        /// </summary>
        public double Coverage
        {
            get
            {
                return QueryMatchLength / Track.TrackLengthSec;
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
        ///  Gets the exact position where resulting track started to match 
        /// </summary>
        internal double QueryMatchStartsAt { get; private set; }

        /// <summary>
        ///   Gets starting position in the origin track that mached at QueryMatchStartPosition
        /// </summary>
        internal double OriginStartsAt { get; private set; }

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