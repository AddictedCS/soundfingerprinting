namespace SoundFingerprinting.Query
{
    using System;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query.Configuration;

    public interface IWithQueryAndFingerprintConfiguration
    {
        IFingerprintQueryCommand WithConfigurations(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration queryConfiguration);

        IFingerprintQueryCommand WithConfigurations<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : IQueryConfiguration, new();

        IFingerprintQueryCommand WithCustomConfigurations(
            Action<CustomFingerprintingConfiguration> fingerprintingConfiguration, Action<CustomQueryConfiguration> queryConfiguration);

        IFingerprintQueryCommand WithDefaultConfigurations();
    }
}