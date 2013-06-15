namespace SoundFingerprinting.Builder
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithFingerprintConfiguration
    {
        IFingerprintUnit With(IFingerprintingConfiguration configuration);

        IFingerprintUnit With<T>() where T : IFingerprintingConfiguration, new();

        IFingerprintUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> functor);

        IFingerprintUnit WithDefaultConfiguration();
    }
}