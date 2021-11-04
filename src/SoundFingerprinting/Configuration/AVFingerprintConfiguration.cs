namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Configuration.Frames;

    public abstract class AVFingerprintConfiguration
    {
        public FingerprintConfiguration Audio { get; set; } = null!;

        public VideoFingerprintConfiguration Video { get; set; } = null!;
    }
}
