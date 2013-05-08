namespace Soundfingerprinting.Query
{
    public class QueryConfiguration : IQueryConfiguration
    {
        public QueryConfiguration(int numberOfHashTables, int numberOfMinhashesPerTable, int thresholdVotes)
        {
            this.NumberOfLSHTables = numberOfHashTables;
            this.NumberOfMinHashesPerTable = numberOfMinhashesPerTable;
            this.ThresholdVotes = thresholdVotes;
        }

        public int NumberOfLSHTables { get; private set; }

        public int NumberOfMinHashesPerTable { get; private set; }

        public int ThresholdVotes { get; private set; }
    }
}