namespace SoundFingerprinting.Configuration
{
    public class AggressiveQueryConfiguration : DefaultQueryConfiguration
    {
        public AggressiveQueryConfiguration()
        {
            FingerprintConfiguration.Stride = QueryStrides.AggressiveStride;
        }
    }
}
