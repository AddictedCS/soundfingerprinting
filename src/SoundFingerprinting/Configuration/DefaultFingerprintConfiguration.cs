namespace SoundFingerprinting.Configuration
{
    using System.Linq;

    public class DefaultFingerprintConfiguration : FingerprintConfiguration
    {
        public DefaultFingerprintConfiguration()
        {
            SpectrogramConfig = new DefaultSpectrogramConfig();
            HashingConfig = new DefaultHashingConfig();
            TopWavelets = 200;
            SampleRate = 5512;
            NormalizeSignal = false;
            Clusters = Enumerable.Empty<string>();
        }
    }
}
