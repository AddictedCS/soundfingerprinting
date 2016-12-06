namespace SoundFingerprinting.Configuration
{
    using System.Linq;

    public class DefaultQueryConfiguration : QueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = 5;
            MaxTracksToReturn = 25;
            Clusters = Enumerable.Empty<string>();
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
        }
    }
}
