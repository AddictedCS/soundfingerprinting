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
        /// Gets starting position of the matched sequence (available only with query with the time sequence information)
        /// </summary>
        public double SequenceStart { get; internal set; }

        /// <summary>
        /// Gets the length of the matched sequence (available only with query with the time sequence information)
        /// </summary>
        public double SequenceLength { get; internal set; }

        /// <summary>
        ///  Gets confidence value [0, 1) (available only with query with the time sequence information). The closer to 1 the bigger confidence on search response.
        /// </summary>
        public double Confidence { get; internal set; }

        /// <summary>
        /// Gets number of matched fingerprints
        /// </summary>
        public int MatchedFingerprints { get; internal set; }
    }
}