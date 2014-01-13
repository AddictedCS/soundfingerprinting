namespace SoundFingerprinting.Dao.Internal
{
    using SoundFingerprinting.Data;

    internal interface ISubFingerprintDao
    {
        SubFingerprintData ReadById(long id);

        long Insert(byte[] signature, int trackId);
    }
}