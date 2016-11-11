namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference);

        IList<HashedFingerprint> ReadHashedFingerprintsByTrackReference(IModelReference trackReference);

        IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashes, int thresholdVotes);

        IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashes, int thresholdVotes, string trackGroupId);

        ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<long[]> hashes, int threshold);
    }
}