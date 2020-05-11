namespace SoundFingerprinting.Configuration
{
    using System;
    using SoundFingerprinting.Configuration.Frames;

    public class DefaultFingerprintConfiguration : FingerprintConfiguration
    {
        public DefaultFingerprintConfiguration()
        {
            SpectrogramConfig = new DefaultSpectrogramConfig();
            HashingConfig = new DefaultHashingConfig();
            TopWavelets = 200;
            SampleRate = 5512;
            HaarWaveletNorm = Math.Sqrt(2);
            FingerprintLengthInSeconds = (double)SamplesPerFingerprint / SampleRate;
            OriginalPointSaveTransform =  (_ => Array.Empty<byte>());
            GaussianBlurConfiguration = GaussianBlurConfiguration.None;
            FrameNormalizationTransform = new LogSpectrumNormalization();
        }
    }
}
