namespace Soundfingerprinting.Query
{
    using System;

    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Query.Configuration;

    public interface IOngoingQueryConfigurationWithFingerprinting
    {
        IFingerprintingQueryUnit With(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration queryConfiguration);

        IFingerprintingQueryUnit With<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : IQueryConfiguration, new();

        IFingerprintingQueryUnit WithCustomConfigurations(
            Action<CustomFingerprintingConfiguration> fingerprintingConfiguration, Action<CustomQueryConfiguration> queryConfiguration);
    }
}