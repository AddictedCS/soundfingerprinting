namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;

    public class HighPrecisionFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public HighPrecisionFingerprintConfiguration()
        {
            Stride = new IncrementalStaticStride(1024);
        }
    }
}
