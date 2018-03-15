namespace SoundFingerprinting.Configuration
{
    public class HighPrecisionQueryConfiguration : DefaultQueryConfiguration
    {
        public HighPrecisionQueryConfiguration()
        {
            FingerprintConfiguration.Stride = QueryStrides.HighPrecisionStride;
            ThresholdVotes = 2;
        }
    }
}
