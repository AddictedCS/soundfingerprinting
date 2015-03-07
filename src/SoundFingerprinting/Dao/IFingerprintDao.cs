namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IFingerprintDao
    {
        IModelReference InsertFingerprint(FingerprintData fingerprint);

        IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference);
    }
} 