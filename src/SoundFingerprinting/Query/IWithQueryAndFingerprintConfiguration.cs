namespace SoundFingerprinting.Query
{
    using System;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query.Configuration;

    public interface IWithQueryAndFingerprintConfiguration
    {
        IFingerprintQueryUnit WithConfigurations(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration queryConfiguration);

        IFingerprintQueryUnit WithConfigurations<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : IQueryConfiguration, new();

        IFingerprintQueryUnit WithCustomConfigurations(
            Action<CustomFingerprintingConfiguration> fingerprintingConfiguration, Action<CustomQueryConfiguration> queryConfiguration);

        IFingerprintQueryUnit WithDefaultConfigurations();
    }
}