namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Configuration.Frames;

    /// <summary>
    ///  Class that hold default properties for Audio/Video fingerprinting.
    /// </summary>
    public class DefaultAVFingerprintConfiguration : AVFingerprintConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAVFingerprintConfiguration"/> class.
        /// </summary>
        public DefaultAVFingerprintConfiguration()
        {
            Audio = new DefaultFingerprintConfiguration();
            Video = new DefaultVideoFingerprintConfiguration();
        }
    }
}