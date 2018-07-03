namespace SoundFingerprinting.Configuration
{
    public class HighPrecisionQueryConfiguration : DefaultQueryConfiguration
    {
        public HighPrecisionQueryConfiguration()
        {
            FrequencyRange = Configs.FrequencyRanges.HighPrecision;
            FingerprintConfiguration.Stride = Configs.QueryStrides.HighPrecisionStride;
            ThresholdVotes = Configs.Threshold.HighPrecision;
        }
    }
}
