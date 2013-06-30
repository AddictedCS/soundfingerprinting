namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Configuration;

    public interface IFingerprintUnit
    {
        IFingerprintingConfiguration Configuration { get; }

        IFingerprinter FingerprintIt();
    }
}