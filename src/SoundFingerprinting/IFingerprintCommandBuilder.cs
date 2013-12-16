namespace SoundFingerprinting
{
    using SoundFingerprinting.Builder;

    public interface IFingerprintCommandBuilder
    {
        ISourceFrom BuildFingerprintCommand();
    }
}