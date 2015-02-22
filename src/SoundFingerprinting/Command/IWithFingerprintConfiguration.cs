namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithFingerprintConfiguration
    {
        IUsingFingerprintServices WithFingerprintConfig(FingerprintConfiguration configuration);

        IUsingFingerprintServices WithFingerprintConfig<T>() where T : FingerprintConfiguration, new();

        IUsingFingerprintServices WithFingerprintConfig(Action<CustomFingerprintConfiguration> functor);

        IUsingFingerprintServices WithDefaultFingerprintConfig();
    }
}