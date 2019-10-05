namespace SoundFingerprinting.Configuration
{
    public class DefaultHashingConfig : HashingConfig
    {
        public DefaultHashingConfig()
        {
            NumberOfLSHTables = 25;
            NumberOfMinHashesPerTable = 4;
            HashBuckets = 0;
            Width = 128;
            Height = 32;
        }
    }
}