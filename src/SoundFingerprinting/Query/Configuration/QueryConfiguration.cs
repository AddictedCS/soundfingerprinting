namespace SoundFingerprinting.Query.Configuration
{
    public class QueryConfiguration : IQueryConfiguration
    {
        public QueryConfiguration(int numberOfHashTables, int numberOfMinhashesPerTable, int thresholdVotes)
        {
            NumberOfLSHTables = numberOfHashTables;
            NumberOfMinHashesPerTable = numberOfMinhashesPerTable;
            ThresholdVotes = thresholdVotes;
        }

        public int NumberOfLSHTables { get; private set; }

        public int NumberOfMinHashesPerTable { get; private set; }

        public int ThresholdVotes { get; private set; }
    }
}