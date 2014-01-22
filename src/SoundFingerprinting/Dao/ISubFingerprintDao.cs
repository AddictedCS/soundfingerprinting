namespace SoundFingerprinting.Dao
{
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        SubFingerprintData ReadById(long id);

        long Insert(byte[] signature, int trackId);
    }
}