namespace SoundFingerprinting.Configuration
{
    public class DefaultQueryConfiguration : QueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = 5;
            MaxTracksToReturn = 25;
            TrackGroupId = string.Empty;
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
        }
    }
}
