namespace SoundFingerprinting.Configuration
{
    public class HighPrecisionFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public HighPrecisionFingerprintConfiguration()
        {
            FrequencyRange = Configs.FrequencyRanges.HighPrecision;
            Stride = Configs.FingerprintStrides.HighPrecision;
        }
    }
}
