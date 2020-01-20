namespace SoundFingerprinting.Query
{
    using System;

    public class MatchedWith : IEquatable<MatchedWith>
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

        public bool Equals(MatchedWith other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return QuerySequenceNumber == other.QuerySequenceNumber && QueryMatchAt.Equals(other.QueryMatchAt) && TrackSequenceNumber == other.TrackSequenceNumber && TrackMatchAt.Equals(other.TrackMatchAt) && Score.Equals(other.Score);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MatchedWith) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) QuerySequenceNumber;
                hashCode = (hashCode * 397) ^ QueryMatchAt.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) TrackSequenceNumber;
                hashCode = (hashCode * 397) ^ TrackMatchAt.GetHashCode();
                hashCode = (hashCode * 397) ^ Score.GetHashCode();
                return hashCode;
            }
        }
    }
}