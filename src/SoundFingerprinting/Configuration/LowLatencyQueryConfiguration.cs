namespace SoundFingerprinting.Configuration
{
    public class LowLatencyQueryConfiguration : DefaultQueryConfiguration
    {
        public LowLatencyQueryConfiguration()
        {
            FingerprintConfiguration.Stride = QueryStrides.LowLatency;
        }
    }
}
