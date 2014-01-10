namespace SoundFingerprinting
{
    using SoundFingerprinting.Command;

    public interface IFingerprintCommandBuilder
    {
        ISourceFrom BuildFingerprintCommand();
    }
}