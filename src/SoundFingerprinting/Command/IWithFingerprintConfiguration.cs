namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithFingerprintConfiguration
    {
        IFingerprintCommand WithFingerprintConfig(IFingerprintConfiguration configuration);

        IFingerprintCommand WithFingerprintConfig<T>() where T : IFingerprintConfiguration, new();

        IFingerprintCommand WithFingerprintConfig(Action<CustomFingerprintConfiguration> functor);

        IFingerprintCommand WithDefaultFingerprintConfig();
    }
}