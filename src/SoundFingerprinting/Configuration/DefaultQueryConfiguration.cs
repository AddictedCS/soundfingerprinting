namespace SoundFingerprinting.Configuration
{
    public class DefaultQueryConfiguration : IQueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = 5;
            MaximumNumberOfTracksToReturnAsResult = 25;
        }

        public int ThresholdVotes { get; protected set; }

        public int MaximumNumberOfTracksToReturnAsResult { get; protected set; }
    }
}
