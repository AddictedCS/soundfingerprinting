namespace SoundFingerprinting.Dao
{
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        SubFingerprintData ReadById(IModelReference subFingerprintReference);

        IModelReference Insert(byte[] signature, IModelReference trackReference);
    }
}