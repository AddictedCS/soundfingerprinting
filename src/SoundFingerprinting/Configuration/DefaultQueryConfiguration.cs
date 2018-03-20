namespace SoundFingerprinting.Configuration
{
    using System.Linq;

    public class DefaultQueryConfiguration : QueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            FrequencyRange = Configs.FrequencyRanges.Default;
            ThresholdVotes = Configs.Threshold.Default;
            MaxTracksToReturn = 25;
            Clusters = Enumerable.Empty<string>();
            FingerprintConfiguration = new DefaultFingerprintConfiguration { Stride = Configs.QueryStrides.DefaultStride };
        }
    }
}
