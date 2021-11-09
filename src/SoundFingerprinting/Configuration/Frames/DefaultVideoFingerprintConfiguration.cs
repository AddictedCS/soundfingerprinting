namespace SoundFingerprinting.Configuration.Frames
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Emy.AudioVideo.Config.Video;

    public class DefaultVideoFingerprintConfiguration : VideoFingerprintConfiguration
    {
        private const int Width = 128;
        private const int Height = 72;
        private const double TopWaveletsPercentage = 0.04;

        public DefaultVideoFingerprintConfiguration()
        {
            HashingConfig = new DefaultHashingConfig
            {
                Width = Width,
                Height = Height
            };
            
            TopWavelets = (int)(TopWaveletsPercentage * Width * Height);

            FrameRate = 30;
            FingerprintLengthInSeconds = 1d / FrameRate;
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

            FrameNormalizationTransform = new NoFrameNormalization();
            GaussianBlurConfiguration = GaussianBlurConfiguration.Default;
        }
    }
}
