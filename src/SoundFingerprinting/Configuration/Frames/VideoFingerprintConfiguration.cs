namespace SoundFingerprinting.Configuration.Frames
{
    using SoundFingerprinting.Configuration;

    public abstract class VideoFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public int FrameRate { get; set; }

        public string AdditionalFilters { get; set; } = null!;

        public CroppingConfiguration CroppingConfiguration { get; set; } = null!;

        public BlackFramesFilterConfiguration BlackFramesFilterConfiguration { get; set; } = null!;
    }
}
