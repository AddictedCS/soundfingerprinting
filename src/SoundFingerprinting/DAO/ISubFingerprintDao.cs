namespace SoundFingerprinting.DAO
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        int SubFingerprintsCount { get; }

        IEnumerable<int> HashCountsPerTable { get; }

        IEnumerable<SubFingerprintData> InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashedFingerprints, IModelReference trackReference);

        void InsertSubFingerprints(IEnumerable<SubFingerprintData> subFingerprints);

        [Obsolete]
        IList<HashedFingerprint> ReadHashedFingerprintsByTrackReference(IModelReference trackReference);

        FingerprintsQueryResponse ReadSubFingerprints(IEnumerable<QueryHash> hashes, QueryConfiguration queryConfiguration);
    }
}