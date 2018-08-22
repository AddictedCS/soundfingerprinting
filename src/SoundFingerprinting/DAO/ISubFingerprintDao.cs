namespace SoundFingerprinting.DAO
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference);

        [Obsolete]
        IList<HashedFingerprint> ReadHashedFingerprintsByTrackReference(IModelReference trackReference);

        ISet<SubFingerprintData> ReadSubFingerprints(
            IEnumerable<int[]> hashes,
            int threshold,
            IEnumerable<string> assignedClusters,
            IDictionary<string, string> metaFields);
    }
}