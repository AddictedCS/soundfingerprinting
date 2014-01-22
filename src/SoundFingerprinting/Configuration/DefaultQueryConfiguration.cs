namespace SoundFingerprinting.Configuration
{
    public class DefaultQueryConfiguration : IQueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = 5;

            MaximumNumberOfTracksToReturnAsResult = 25;
        }

        public int ThresholdVotes { get; private set; }

        public int MaximumNumberOfTracksToReturnAsResult { get; private set; }
    }
}
