namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IFingerprintDao
    {
        IModelReference Insert(bool[] signature, IModelReference trackId);

        IList<FingerprintData> ReadFingerprintsByTrackId(IModelReference trackId);
    }
} 