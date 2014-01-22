namespace SoundFingerprinting.Configuration
{
    public class CustomQueryConfiguration : IQueryConfiguration
    {
        public CustomQueryConfiguration()
        {
            var defaultQueryConfig = new DefaultQueryConfiguration();
            ThresholdVotes = defaultQueryConfig.ThresholdVotes;
            MaximumNumberOfTracksToReturnAsResult = defaultQueryConfig.MaximumNumberOfTracksToReturnAsResult;
        }

        public int ThresholdVotes { get; set; }

        public int MaximumNumberOfTracksToReturnAsResult { get; set; }
    }
}
