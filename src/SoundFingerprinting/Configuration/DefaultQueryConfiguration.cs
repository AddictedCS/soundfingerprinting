namespace SoundFingerprinting.Configuration
{
    public class DefaultQueryConfiguration : IQueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = 5;
            MaximumNumberOfTracksToReturnAsResult = 25;
            TrackGroupId = string.Empty;
        }

        public int ThresholdVotes { get; protected set; }

        public int MaximumNumberOfTracksToReturnAsResult { get; protected set; }

        public string TrackGroupId { get; protected set; }
    }
}
