namespace SoundFingerprinting.Configuration.Frames
{
    using System;
    using SoundFingerprinting.Configuration;

    /// <summary>
    ///  Default fingerprint configuration class, defining default parameters used to fingerprint provided video content.
    /// </summary>
    public class DefaultVideoFingerprintConfiguration : VideoFingerprintConfiguration
    {
        private const int Width = 128;
        private const int Height = 72;
        private const double TopWaveletsPercentage = 0.04;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultVideoFingerprintConfiguration"/> class.
        /// </summary>
        public DefaultVideoFingerprintConfiguration()
        {
            SpectrogramConfig = new DefaultSpectrogramConfig(); // not used but still initialized to not have it as null
            SampleRate = 1;                                     // not used but still initialized to not have it as null
            
            HaarWaveletNorm = Math.Sqrt(2);
            OriginalPointSaveTransform =  _ => Array.Empty<byte>();
            
            HashingConfig = new DefaultHashingConfig
            {
                Width = Width,
                Height = Height
            };

            TopWavelets = (int)(TopWaveletsPercentage * Width * Height);

            FrameRate = 30;
            AdditionalFilters = "";

            CroppingConfiguration = new CroppingConfiguration
            {
                Detector = CropDetector.BBox,
                Parameters = "min_val=24"
            };

            BlackFramesFilterConfiguration = new BlackFramesFilterConfiguration
            {
                Threshold = 32,
                Amount = 98
            };

            FrameNormalizationTransform = new GaussianBlurFrameNormalization(GaussianBlurConfiguration.Default);
        }
    }
}
