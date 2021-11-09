namespace SoundFingerprinting.Configuration.Frames
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Emy.AudioVideo.Config.Video;

    public abstract class VideoFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public int FrameRate { get; set; }

        public string AdditionalFilters { get; set; } = null!;

        public CroppingConfiguration CroppingConfiguration { get; set; } = null!;

        public BlackFramesFilterConfiguration BlackFramesFilterConfiguration { get; set; } = null!;
    }
}
