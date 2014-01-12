namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithQueryAndFingerprintConfiguration
    {
        IQueryCommand WithConfigs(IFingerprintConfiguration fingerprintConfiguration, IQueryConfiguration queryConfiguration);

        IQueryCommand WithConfigs<T1, T2>() where T1 : IFingerprintConfiguration, new() where T2 : IQueryConfiguration, new();

        IQueryCommand WithConfigs(Action<CustomFingerprintConfiguration> fingerprintConfiguration, Action<CustomQueryConfiguration> queryConfiguration);

        IQueryCommand WithDefaultConfigs();
    }
}