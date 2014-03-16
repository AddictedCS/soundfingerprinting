namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithFingerprintConfiguration
    {
        IUsingFingerprintServices WithFingerprintConfig(IFingerprintConfiguration configuration);

        IUsingFingerprintServices WithFingerprintConfig<T>() where T : IFingerprintConfiguration, new();

        IUsingFingerprintServices WithFingerprintConfig(Action<CustomFingerprintConfiguration> functor);

        IUsingFingerprintServices WithDefaultFingerprintConfig();
    }
}