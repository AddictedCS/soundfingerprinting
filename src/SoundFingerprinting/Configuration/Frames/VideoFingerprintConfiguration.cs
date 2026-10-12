namespace SoundFingerprinting.Configuration.Frames
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Video;

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
        ///  Gets or sets the sink that takes crops of a frame region while the source is read, or null when nothing is cropped.
        /// </summary>
        /// <remarks>
        ///  A media service that supports it reads <see cref="IFrameCropSink.Region"/> once at the start of every source and crops the decoded frames at the region's rate, next to the fingerprinting frames.
        /// </remarks>
        public IFrameCropSink? FrameCropSink { get; set; }

        /// <summary>
        ///  Gets fingerprint length in seconds.
        /// </summary>
        public override double FingerprintLengthInSeconds => 1d / FrameRate;

        /// <summary>
        ///  Creates a shallow copy of this configuration with another crop sink; this configuration is not changed.
        /// </summary>
        /// <param name="frameCropSink">Sink of the copy, or null for no cropping.</param>
        /// <returns>The copy.</returns>
        public VideoFingerprintConfiguration WithFrameCropSink(IFrameCropSink? frameCropSink)
        {
            var copy = (VideoFingerprintConfiguration)MemberwiseClone();
            copy.FrameCropSink = frameCropSink;
            return copy;
        }
    }
}
