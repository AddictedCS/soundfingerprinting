namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Configuration.Frames;

    /// <summary>
    ///  Class that hold default properties for Audio/Video query.
    /// </summary>
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
