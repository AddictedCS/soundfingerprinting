namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;

    public interface IUsingFingerprintServices
    {
        IFingerprintCommand UsingServices(IAudioService audioService);
    }
}