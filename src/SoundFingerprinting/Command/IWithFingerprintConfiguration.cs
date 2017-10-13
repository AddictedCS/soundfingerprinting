namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithFingerprintConfiguration : IUsingFingerprintServices
    {
        IUsingFingerprintServices WithFingerprintConfig(FingerprintConfiguration configuration);

        IUsingFingerprintServices WithFingerprintConfig(Func<FingerprintConfiguration, FingerprintConfiguration> amendFunctor);
    }
}