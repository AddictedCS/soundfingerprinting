namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    public interface IFingerprintCommandBuilder
    {
        ISourceFrom BuildFingerprintCommand();
    }
}