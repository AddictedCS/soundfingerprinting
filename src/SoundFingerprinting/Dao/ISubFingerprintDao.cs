namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference);

        IModelReference InsertSubFingerprint(long[] hashes, int sequenceNumber, double sequenceAt, IModelReference trackReference);

        IList<HashedFingerprint> ReadHashedFingerprintsByTrackReference(IModelReference trackReference);

        IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashes, int thresholdVotes);

        IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashes, int thresholdVotes, string trackGroupId);

        ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<long[]> hashes, int threshold);
    }
}