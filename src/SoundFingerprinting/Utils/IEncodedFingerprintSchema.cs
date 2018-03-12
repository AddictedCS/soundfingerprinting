namespace SoundFingerprinting.Utils
{
    public interface IEncodedFingerprintSchema
    {
        bool IsTrueAt(int index);

        bool IsSilence();

        bool[] ToBools();
    }
}