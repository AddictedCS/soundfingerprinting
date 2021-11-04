namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Configuration.Frames;

    public class DefaultAVQueryConfiguration : AVQueryConfiguration
    {
        public DefaultAVQueryConfiguration()
        {
            Audio = new DefaultQueryConfiguration
            {
                PermittedGap = 3
            };
            
            Video = new DefaultVideoQueryConfiguration();
        }
    }
}
