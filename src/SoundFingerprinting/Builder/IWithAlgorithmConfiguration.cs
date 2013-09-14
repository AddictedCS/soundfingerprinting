namespace SoundFingerprinting.Builder
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithAlgorithmConfiguration
    {
        IAudioFingerprintingUnit WithAlgorithmConfiguration(IFingerprintingConfiguration configuration);

        IAudioFingerprintingUnit WithAlgorithmConfiguration<T>() where T : IFingerprintingConfiguration, new();

        IAudioFingerprintingUnit WithCustomAlgorithmConfiguration(Action<CustomFingerprintingConfiguration> functor);

        IAudioFingerprintingUnit WithDefaultAlgorithmConfiguration();
    }
}