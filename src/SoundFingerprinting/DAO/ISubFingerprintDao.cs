namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        int SubFingerprintsCount { get; }

        IEnumerable<int> HashCountsPerTable { get; }

        void InsertSubFingerprints(IEnumerable<SubFingerprintData> subFingerprints);

        IEnumerable<SubFingerprintData> ReadHashedFingerprintsByTrackReference(IModelReference trackReference);

        IEnumerable<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, QueryConfiguration queryConfiguration);

        int DeleteSubFingerprintsByTrackReference(IModelReference trackReference);
    }
}