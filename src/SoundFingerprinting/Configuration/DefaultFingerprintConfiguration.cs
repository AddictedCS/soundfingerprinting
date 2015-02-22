namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;
    
    public class DefaultFingerprintConfiguration : FingerprintConfiguration
    {
        public DefaultFingerprintConfiguration()
        {
            SpectrogramConfig = SpectrogramConfig.Default;
            HashingConfig = HashingConfig.Default;
            TopWavelets = 200;
            SampleRate = 5512;
            Stride = new IncrementalStaticStride(5115, SpectrogramConfig.ImageLength * SpectrogramConfig.Overlap);
            NormalizeSignal = false;
        }
    }
}
