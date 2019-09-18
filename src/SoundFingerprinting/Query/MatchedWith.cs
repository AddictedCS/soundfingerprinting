namespace SoundFingerprinting.Query
{
    public class MatchedWith
    {
        public MatchedWith(uint querySequenceNumber, float queryMatchAt, uint trackSequenceNumber, float trackMatchAt, double score)
        {
            QuerySequenceNumber = querySequenceNumber;
            QueryMatchAt = queryMatchAt;
            TrackMatchAt = trackMatchAt;
            TrackSequenceNumber = trackSequenceNumber;
            Score = score;
        }

        public uint QuerySequenceNumber { get; }

        public float QueryMatchAt { get; }

        public uint TrackSequenceNumber { get; }

        public float TrackMatchAt { get; }

        public double Score { get; }
    }
}