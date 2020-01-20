namespace SoundFingerprinting.LCS
{
    using System;
    using SoundFingerprinting.Query;

    internal class MaxAt : IEquatable<MaxAt>
    {
        public MaxAt(int length, MatchedWith matchedWith)
        {
            Length = length;
            MatchedWith = matchedWith;
        }

        public int Length { get; }
        
        public MatchedWith MatchedWith { get; }

        public bool Equals(MaxAt other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Length == other.Length && Equals(MatchedWith, other.MatchedWith);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MaxAt) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Length * 397) ^ (MatchedWith != null ? MatchedWith.GetHashCode() : 0);
            }
        }
    }
}