namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;

    public interface IFingerprintDao
    {
        IModelReference InsertFingerprint(FingerprintData fingerprint);

        IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference);
    }
}