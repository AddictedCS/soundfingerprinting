namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;

    public class ResultEntry
    {
        /// <summary>
        /// Gets the resulting track
        /// </summary>
        public TrackData Track { get; internal set; }

        /// <summary>
        /// Gets starting position of the matched sequence (available only with query with time sequence information method)
        /// </summary>
        public double SequenceStart { get; internal set; }

        /// <summary>
        /// Gets the length of the matched sequence (available only with query with time sequence information method)
        /// </summary>
        public double SequenceLength { get; internal set; }

        /// <summary>
        ///  Gets confidence marker [0, 1) (available only with query with time sequence information method)
        /// </summary>
        public double Confidence { get; internal set; }

        /// <summary>
        /// Gets number of matched fingerprints
        /// </summary>
        public int MatchedFingerprints { get; internal set; }
    }
}