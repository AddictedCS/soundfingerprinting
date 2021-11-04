namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Configuration.Frames;
    using SoundFingerprinting.Emy.AudioVideo.Config;

    public abstract class AVQueryConfiguration
    {
        public QueryConfiguration Audio { get; set; } = null!;

        public VideoQueryConfiguration Video { get; set; } = null!;

        public AVFingerprintConfiguration FingerprintConfiguration => new DefaultAVFingerprintConfiguration
        {
            Audio = Audio.FingerprintConfiguration,
            Video = (VideoFingerprintConfiguration)Video.FingerprintConfiguration
        };
    }
}
