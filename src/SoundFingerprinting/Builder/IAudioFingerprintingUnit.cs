namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Configuration;

    public interface IAudioFingerprintingUnit
    {
        IFingerprintingConfiguration Configuration { get; }

        IFingerprinter FingerprintIt();
    }
}