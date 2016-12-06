namespace SoundFingerprinting.Query
{
    using System;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal class MatchedPair : IComparable<MatchedPair>
    {
        public MatchedPair(HashedFingerprint hashedFingerprint, SubFingerprintData subFingerprint, int hammingSimilarity)
        {
            HashedFingerprint = hashedFingerprint;
            SubFingerprint = subFingerprint;
            HammingSimilarity = hammingSimilarity;
        }

        public SubFingerprintData SubFingerprint { get; private set; }

        public HashedFingerprint HashedFingerprint { get; private set; }

        public int HammingSimilarity { get; private set; }

        public int CompareTo(MatchedPair other)
        {
            return SubFingerprint.SequenceAt.CompareTo(other.SubFingerprint.SequenceAt);
        }
    }
}
