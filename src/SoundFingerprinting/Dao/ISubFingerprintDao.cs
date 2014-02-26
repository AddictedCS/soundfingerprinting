namespace SoundFingerprinting.Dao
{
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        SubFingerprintData Read(IModelReference subFingerprintReference);

        IModelReference InsertSubFingerprint(byte[] signature, IModelReference trackReference);
    }
}