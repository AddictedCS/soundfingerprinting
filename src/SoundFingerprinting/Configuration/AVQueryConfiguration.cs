namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Configuration.Frames;

    /// <summary>
    ///  Class that hold default properties for Audio/Video query configuration.
    /// </summary>
    public abstract class AVQueryConfiguration
    {
        /// <summary>
        ///  Gets or sets audio query configuration.
        /// </summary>
        public QueryConfiguration Audio { get; set; } = null!;

        /// <summary>
        ///  Gets or sets video query configuration.
        /// </summary>
        public VideoQueryConfiguration Video { get; set; } = null!;

        /// <summary>
        ///  Gets or sets AV fingerprint configuration.
        /// </summary>
        public AVFingerprintConfiguration FingerprintConfiguration => new DefaultAVFingerprintConfiguration
        {
            Audio = Audio.FingerprintConfiguration,
            Video = (VideoFingerprintConfiguration)Video.FingerprintConfiguration
        };
    }
}
