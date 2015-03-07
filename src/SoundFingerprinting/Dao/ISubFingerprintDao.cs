namespace SoundFingerprinting.DAO
{
    using SoundFingerprinting.Data;

    public interface ISubFingerprintDao
    {
        SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference);

        IModelReference InsertSubFingerprint(byte[] signature, int sequenceNumber, double sequenceAt, IModelReference trackReference);
    }
}