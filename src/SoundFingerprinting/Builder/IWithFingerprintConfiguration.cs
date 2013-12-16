namespace SoundFingerprinting.Builder
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithFingerprintConfiguration
    {
        IFingerprintCommand WithAlgorithmConfiguration(IFingerprintingConfiguration configuration);

        IFingerprintCommand WithAlgorithmConfiguration<T>() where T : IFingerprintingConfiguration, new();

        IFingerprintCommand WithCustomAlgorithmConfiguration(Action<CustomFingerprintingConfiguration> functor);

        IFingerprintCommand WithDefaultAlgorithmConfiguration();
    }
}