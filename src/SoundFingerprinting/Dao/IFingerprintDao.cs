namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal interface IFingerprintDao
    {
        int Insert(bool[] signature, int trackId);

        IList<FingerprintData> ReadFingerprintsByTrackId(int trackId);
    }
}