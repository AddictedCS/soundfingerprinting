namespace SoundFingerprinting.Configuration
{
    public class DefaultHashingConfig : HashingConfig
    {
        public DefaultHashingConfig()
        {
            NumberOfLSHTables = 25;
            NumberOfMinHashesPerTable = 4;
        }
    }
}