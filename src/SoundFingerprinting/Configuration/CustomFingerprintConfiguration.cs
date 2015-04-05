namespace SoundFingerprinting.Configuration
{
    public class CustomFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public CustomFingerprintConfiguration()
        {
            SpectrogramConfig = new CustomSpectrogramConfig();
            HashingConfig = new CustomHashingConfig();
        }
    }
}
