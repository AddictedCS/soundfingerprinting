namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;

    public class AggressiveFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public AggressiveFingerprintConfiguration()
        {
            Stride = new IncrementalStaticStride(512);
        }
    }
}
