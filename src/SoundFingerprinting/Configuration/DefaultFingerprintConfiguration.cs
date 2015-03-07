namespace SoundFingerprinting.Configuration
{
    public class DefaultFingerprintConfiguration : FingerprintConfiguration
    {
        public DefaultFingerprintConfiguration()
        {
            SpectrogramConfig = SpectrogramConfig.Default;
            HashingConfig = HashingConfig.Default;
            TopWavelets = 200;
            SampleRate = 5512;
            NormalizeSignal = false;
        }
    }
}
