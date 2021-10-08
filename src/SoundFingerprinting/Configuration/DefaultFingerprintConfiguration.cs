namespace SoundFingerprinting.Configuration
{
    using System;
    using SoundFingerprinting.Configuration.Frames;

    /// <summary>
    ///  Default fingerprint configuration class, defining default parameters used to fingerprint provided content.
    /// </summary>
    public class DefaultFingerprintConfiguration : FingerprintConfiguration
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="DefaultFingerprintConfiguration"/> class.
        /// </summary>
        public DefaultFingerprintConfiguration()
        {
            SpectrogramConfig = new DefaultSpectrogramConfig();
            HashingConfig = new DefaultHashingConfig();
            TopWavelets = 200;
            SampleRate = 5512;
            HaarWaveletNorm = Math.Sqrt(2);
            FingerprintLengthInSeconds = (double)SamplesPerFingerprint / SampleRate;
            OriginalPointSaveTransform =  _ => Array.Empty<byte>();
            GaussianBlurConfiguration = GaussianBlurConfiguration.None;
            FrameNormalizationTransform = new LogSpectrumNormalization();
        }
    }
}
