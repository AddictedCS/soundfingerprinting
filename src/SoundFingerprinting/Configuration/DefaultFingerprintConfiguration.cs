namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Configuration.Frames;

    public class DefaultFingerprintConfiguration : FingerprintConfiguration
    {
        public DefaultFingerprintConfiguration()
        {
            SpectrogramConfig = new DefaultSpectrogramConfig();
            HashingConfig = new DefaultHashingConfig();
            TopWavelets = 200;
            SampleRate = 5512;
            HaarWaveletNorm = System.Math.Sqrt(2);
            FingerprintLengthInSeconds = (double)SamplesPerFingerprint / SampleRate;
            OriginalPointSaveTransform = null;
            GaussianBlurConfiguration = GaussianBlurConfiguration.None;
            FrameNormalizationTransform = new LogSpectrumNormalization();
        }
    }
}
