namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;

    public class ResultEntry
    {
        public ResultEntry(TrackData track, double sequenceAt, double sequenceLength, double confidence, int hammingSimilarity)
        {
            Track = track;
            SequenceAt = sequenceAt;
            SequenceLength = sequenceLength;
            Confidence = confidence;
            HammingSimilarity = hammingSimilarity;
        }

        /// <summary>
        /// Gets the resulting track
        /// </summary>
        public TrackData Track { get; private set; }

        /// <summary>
        /// Gets starting position of the matched sequence (available only with query with the time sequence information)
        /// </summary>
        public double SequenceAt { get; private set; }

        /// <summary>
        /// Gets the length of the matched sequence (available only with query with the time sequence information)
        /// </summary>
        public double SequenceLength { get; private set; }

        /// <summary>
        ///  Gets confidence value [0, 1) (available only with query with the time sequence information). The closer to 1 the bigger confidence on search response.
        /// </summary>
        public double Confidence { get; private set; }

        /// <summary>
        /// Gets number of matched fingerprints
        /// </summary>
        public int HammingSimilarity { get; private set; }
    }
}