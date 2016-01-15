namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;

    public class EfficientFingerprintConfigurationForQuerying : DefaultFingerprintConfiguration
    {
        public EfficientFingerprintConfigurationForQuerying()
        {
            // Empirically determined as a good value for creating
            // the fingerprints for querying
            SpectrogramConfig.Stride = new IncrementalRandomStride(256, 512, SamplesPerFingerprint);
        }
    }
}
