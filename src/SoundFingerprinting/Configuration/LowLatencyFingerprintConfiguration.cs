namespace SoundFingerprinting.Configuration
{
    public class LowLatencyFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public LowLatencyFingerprintConfiguration()
        {
            FrequencyRange = Configs.FrequencyRanges.LowLatency;
            Stride = Configs.FingerprintStrides.LowLatency;
        }
    }
}
