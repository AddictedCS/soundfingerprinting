namespace SoundFingerprinting.Query.Configuration
{
    public class DefaultQueryConfiguration : IQueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = 5;
        }

        public int ThresholdVotes { get; private set; }
    }
}
