namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Configuration.Frames;

    public class DefaultAVFingerprintConfiguration : AVFingerprintConfiguration
    {
        public DefaultAVFingerprintConfiguration()
        {
            Audio = new DefaultFingerprintConfiguration();
            Video = new DefaultVideoFingerprintConfiguration();
        }
    }
}
