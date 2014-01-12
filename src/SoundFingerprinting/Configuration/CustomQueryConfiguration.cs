namespace SoundFingerprinting.Configuration
{
    public class CustomQueryConfiguration : IQueryConfiguration
    {
        public CustomQueryConfiguration()
        {
            ThresholdVotes = new DefaultQueryConfiguration().ThresholdVotes;
        }

        public int ThresholdVotes { get; set; }
    }
}
