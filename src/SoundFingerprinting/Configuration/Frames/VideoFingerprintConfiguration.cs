namespace SoundFingerprinting.Configuration.Frames
{
    using SoundFingerprinting.Configuration;

    /// <summary>
    ///  Video fingerprint configuration class, that defines query parameters specific for video fingerprinting.
    /// </summary>
    public abstract class VideoFingerprintConfiguration : FingerprintConfiguration
    {
        /// <summary>
        ///  Gets or sets video frame rate used to create fingerprints from video source.
        /// </summary>
        public int FrameRate { get; set; }

        /// <summary>
        ///  Gets or sets additional filters used during video fingerprinting process.
        /// </summary>
        public string AdditionalFilters { get; set; } = null!;

        /// <summary>
        ///  Gets or sets video frame cropping configuration.
        /// </summary>
        public CroppingConfiguration CroppingConfiguration { get; set; } = null!;

        /// <summary>
        ///  Gets or sets video frame black frame filter configuration.
        /// </summary>
        public BlackFramesFilterConfiguration BlackFramesFilterConfiguration { get; set; } = null!;
        
        /// <summary>
        ///  Gets fingerprint length in seconds.
        /// </summary>
        public override double FingerprintLengthInSeconds => 1d / FrameRate;
    }
}
