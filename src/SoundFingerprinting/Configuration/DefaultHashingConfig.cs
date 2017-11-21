namespace SoundFingerprinting.Configuration
{
    internal class DefaultHashingConfig : HashingConfig
    {
        public DefaultHashingConfig()
        {
            NumberOfLSHTables = 25;
            NumberOfMinHashesPerTable = 4;
            HashBuckets = 0;
        }
    }
}