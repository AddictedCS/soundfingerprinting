namespace SoundFingerprinting.DAO
{
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference);

        IModelReference InsertSubFingerprint(byte[] signature, IModelReference trackReference);
    }
}