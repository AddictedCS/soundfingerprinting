namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;

    public class LowLatencyFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public LowLatencyFingerprintConfiguration()
        {
            Stride = new IncrementalStaticStride(5115);
        }
    }
}
