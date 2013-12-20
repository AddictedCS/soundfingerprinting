namespace SoundFingerprinting.Query.Configuration
{
    public class CustomQueryConfiguration : IQueryConfiguration
    {
        public CustomQueryConfiguration()
        {
            DefaultQueryConfiguration defaultConfiguration = new DefaultQueryConfiguration();
            NumberOfLSHTables = defaultConfiguration.NumberOfLSHTables;
            NumberOfMinHashesPerTable = defaultConfiguration.NumberOfMinHashesPerTable;
            ThresholdVotes = defaultConfiguration.ThresholdVotes;
            MaximumNumberOfTracksToReturnAsResult = defaultConfiguration.MaximumNumberOfTracksToReturnAsResult;
        }

        public int NumberOfLSHTables { get; set; }

        public int NumberOfMinHashesPerTable { get; set; }

        public int ThresholdVotes { get; set; }

        public int MaximumNumberOfTracksToReturnAsResult { get; set; }
    }
}
