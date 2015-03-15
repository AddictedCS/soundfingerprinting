namespace SoundFingerprinting.Configuration
{
    public class DefaultQueryConfiguration : QueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = 5;
            MaximumNumberOfTracksToReturnAsResult = 25;
            TrackGroupId = string.Empty;
        }
    }
}
