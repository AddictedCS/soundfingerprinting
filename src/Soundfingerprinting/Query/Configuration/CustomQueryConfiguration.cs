namespace Soundfingerprinting.Query.Configuration
{
    public class CustomQueryConfiguration : IQueryConfiguration
    {
        public CustomQueryConfiguration()
        {
            DefaultQueryConfiguration defaultConfiguration = new DefaultQueryConfiguration();
            NumberOfLSHTables = defaultConfiguration.NumberOfLSHTables;
            NumberOfMinHashesPerTable = defaultConfiguration.NumberOfMinHashesPerTable;
        }

        public int NumberOfLSHTables { get; set; }

        public int NumberOfMinHashesPerTable { get; set; }

        public int ThresholdVotes { get; set; }
    }
}
