namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IFingerprintDao
    {
        IModelReference InsertFingerprint(bool[] signature, IModelReference trackReference);

        IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference);
    }
} 