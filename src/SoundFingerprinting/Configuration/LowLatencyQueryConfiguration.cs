namespace SoundFingerprinting.Configuration
{
    public class LowLatencyQueryConfiguration : DefaultQueryConfiguration
    {
        public LowLatencyQueryConfiguration()
        {
            FrequencyRange = Configs.FrequencyRanges.LowLatency;
            ThresholdVotes = Configs.Threshold.LowLatency;
            FingerprintConfiguration.Stride = Configs.QueryStrides.LowLatency;
        }
    }
}
