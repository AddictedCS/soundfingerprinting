namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration.Frames;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Class definition for Audio/Video fingerprinting configuration.
    /// </summary>
    public abstract class AVFingerprintConfiguration
    {
        /// <summary>
        ///  Gets or sets an instance of <see cref="FingerprintConfiguration"/> used for audio fingerprinting.
        /// </summary>
        public FingerprintConfiguration Audio { get; set; } = null!;

        /// <summary>
        ///  Gets or sets an instance of <see cref="VideoFingerprintConfiguration"/> used for video fingerprinting.
        /// </summary>
        public VideoFingerprintConfiguration Video { get; set; } = null!;

        /// <summary>
        ///  Gets an instance of <see cref="AVTrackReadConfiguration"/> used to read <see cref="AudioSamples"/> and <see cref="Frames"/> from the source.
        /// </summary>
        /// <returns>An instance of the <see cref="AVTrackReadConfiguration"/> class.</returns>
        public AVTrackReadConfiguration GetTrackReadConfiguration()
        {
            return new AVTrackReadConfiguration(new AudioTrackReadConfiguration(Audio.SampleRate),
                new VideoTrackReadConfiguration(Video.HashingConfig.Height,
                    Video.HashingConfig.Width,
                    Video.FrameRate,
                    Video.BlackFramesFilterConfiguration,
                    Video.CroppingConfiguration,
                    Video.AdditionalFilters));
        }
    }
}