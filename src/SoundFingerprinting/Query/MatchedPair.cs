namespace SoundFingerprinting.Query
{
    using System.Collections.Concurrent;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;

    internal class MatchedPair
    {
        public MatchedPair(HashedFingerprint hashedFingerprint, SubFingerprintData subFingerprint, int hammingSimilarity)
        {
            HashedFingerprint = hashedFingerprint;
            Matches = new ConcurrentDictionary<IModelReference, MatchedWith>();
            Matches.TryAdd(subFingerprint.TrackReference, new MatchedWith(hashedFingerprint.StartsAt, subFingerprint.SequenceAt, hammingSimilarity));
        }

        public ConcurrentDictionary<IModelReference, MatchedWith> Matches { get; }

        public HashedFingerprint HashedFingerprint { get; }
    }
}
