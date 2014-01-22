namespace SoundFingerprinting.Configuration
{
    public class QueryConfiguration : IQueryConfiguration
    {
        public QueryConfiguration(int thresholdVotes, int maximumNumberOfTracksToReturn)
        {
            ThresholdVotes = thresholdVotes;
            MaximumNumberOfTracksToReturnAsResult = maximumNumberOfTracksToReturn;
        }

        public int ThresholdVotes { get; private set; }

        public int MaximumNumberOfTracksToReturnAsResult { get; private set; }
    }
}