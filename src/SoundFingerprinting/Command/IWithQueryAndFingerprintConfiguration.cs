namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithQueryAndFingerprintConfiguration : IUsingQueryServices
    {
        IUsingQueryServices WithFingerprintConfig(FingerprintConfiguration fingerprintConfiguration);

        IUsingQueryServices WithQueryConfig(QueryConfiguration queryConfiguration);

        IUsingQueryServices WithConfigs(FingerprintConfiguration fingerprintConfiguration, QueryConfiguration queryConfiguration);

        IUsingQueryServices WithConfigs<T1, T2>() where T1 : FingerprintConfiguration, new() where T2 : QueryConfiguration, new();

        IUsingQueryServices WithConfigs(Action<FingerprintConfiguration> fingerprintConfiguration, Action<QueryConfiguration> queryConfiguration);
    }
}