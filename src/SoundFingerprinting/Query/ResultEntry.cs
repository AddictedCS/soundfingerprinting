namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;

    public class ResultEntry
    {
        public ResultEntry(TrackData track, double startsAt, double length, double originStartsAt, double trackStartsAt, double confidence, int hammingSimilaritySum)
        {
            Track = track;
            this.StartsAt = startsAt;
            this.OriginStartsAt = originStartsAt;
            this.Length = length;
            Confidence = confidence;
            HammingSimilaritySum = hammingSimilaritySum;
            TrackStartsAt = trackStartsAt;
        }

        /// <summary>
        ///  Gets the resulting track
        /// </summary>
        public TrackData Track { get; private set; }

        /// <summary>
        ///  Gets starting position of the query sequence
        /// </summary>
        public double StartsAt { get; private set; }

        /// <summary>
        ///  Gets starting position in the origin track
        /// </summary>
        public double OriginStartsAt { get; private set; }

        /// <summary>
        ///  Gets starting point of the result track in the source query
        /// </summary>
        public double TrackStartsAt { get; private set; }

        /// <summary>
        ///  Gets the exact length of the matched sequence
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        ///  Gets the exact percentange of how much the result entry covered comparing to the original song.
        /// </summary>
        public double Coverage
        {
            get
            {
                return Length / Track.TrackLengthSec;
            }
        }

        /// <summary>
        ///  Gets confidence value [0, 1) 
        /// </summary>
        public double Confidence { get; private set; }

        /// <summary>
        ///  Gets number of matched fingerprints
        /// </summary>
        public int HammingSimilaritySum { get; private set; }

        internal MatchedPair BestMatch { get; set; }
    }
}