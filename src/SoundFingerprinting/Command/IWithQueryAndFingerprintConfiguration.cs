namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    public interface IWithQueryAndFingerprintConfiguration : IUsingQueryServices
    {
        IUsingQueryServices WithFingerprintConfig(FingerprintConfiguration fingerprintConfiguration);

        IUsingQueryServices WithFingerprintConfig(Action<FingerprintConfiguration> amendFingerprintConfigFunctor);

        IUsingQueryServices WithQueryConfig(QueryConfiguration queryConfiguration);

        IUsingQueryServices WithQueryConfig(Action<QueryConfiguration> amendQueryConfigFunctor);

        IUsingQueryServices WithConfigs(FingerprintConfiguration fingerprintConfiguration, QueryConfiguration queryConfiguration);

        IUsingQueryServices WithConfigs(Action<FingerprintConfiguration> amendFingerprintFunctor, Action<QueryConfiguration> amendQueryConfigFunctor);
    }
}