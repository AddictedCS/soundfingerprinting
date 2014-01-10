namespace SoundFingerprinting.Configuration
{
    public class QueryConfiguration : IQueryConfiguration
    {
        public QueryConfiguration(int thresholdVotes)
        {
            ThresholdVotes = thresholdVotes;
        }

        public int ThresholdVotes { get; private set; }
    }
}