namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IFingerprintDao
    {
        IModelReference InsertFingerprint(FingerprintData fingerprint);

        IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference);
    }
} 