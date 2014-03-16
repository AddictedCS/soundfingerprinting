namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithQueryAndFingerprintConfiguration
    {
        IUsingQueryServices WithConfigs(IFingerprintConfiguration fingerprintConfiguration, IQueryConfiguration queryConfiguration);

        IUsingQueryServices WithConfigs<T1, T2>() where T1 : IFingerprintConfiguration, new() where T2 : IQueryConfiguration, new();

        IUsingQueryServices WithConfigs(Action<CustomFingerprintConfiguration> fingerprintConfiguration, Action<CustomQueryConfiguration> queryConfiguration);

        IUsingQueryServices WithDefaultConfigs();
    }
}