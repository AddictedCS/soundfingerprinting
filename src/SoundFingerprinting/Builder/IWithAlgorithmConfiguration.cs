namespace SoundFingerprinting.Builder
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithAlgorithmConfiguration
    {
        IFingerprintUnit WithAlgorithmConfiguration(IFingerprintingConfiguration configuration);

        IFingerprintUnit WithAlgorithmConfiguration<T>() where T : IFingerprintingConfiguration, new();

        IFingerprintUnit WithCustomAlgorithmConfiguration(Action<CustomFingerprintingConfiguration> functor);

        IFingerprintUnit WithDefaultAlgorithmConfiguration();
    }
}