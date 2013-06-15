namespace SoundFingerprinting.Query
{
    using System;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query.Configuration;

    public interface IWithQueryAndFingerprintConfiguration
    {
        IFingerprintQueryUnit With(IFingerprintingConfiguration fingerprintingConfiguration, Configuration.IQueryConfiguration queryConfiguration);

        IFingerprintQueryUnit With<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : Configuration.IQueryConfiguration, new();

        IFingerprintQueryUnit WithCustomConfigurations(
            Action<CustomFingerprintingConfiguration> fingerprintingConfiguration, Action<CustomQueryConfiguration> queryConfiguration);

        IFingerprintQueryUnit WithDefaultConfigurations();
    }
}