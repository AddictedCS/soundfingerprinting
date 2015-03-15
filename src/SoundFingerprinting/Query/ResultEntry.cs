namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;

    public class ResultEntry
    {
        public TrackData Track { get; set; }

        public int Similarity { get; set; }

        public double SequenceStart { get; set; }

        public double SequenceLength { get; set; }
    }
}