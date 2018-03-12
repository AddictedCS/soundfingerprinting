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
            HaarWaveletNorm = 1;
            Clusters = Enumerable.Empty<string>();
        }
    }
}
