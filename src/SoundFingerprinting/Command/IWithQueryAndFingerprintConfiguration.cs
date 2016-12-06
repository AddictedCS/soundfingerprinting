namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    /// <summary>
    ///   Configuration ammender interface
    /// </summary>
    public interface IWithQueryAndFingerprintConfiguration : IUsingQueryServices
    {
        /// <summary>
        ///   Sets fingerprint configuration used to generate fingerprints for querying
        /// </summary>
        /// <param name="fingerprintConfiguration">Fingerprint configuration to use</param>
        /// <returns>Query services selector</returns>
        IUsingQueryServices WithFingerprintConfig(FingerprintConfiguration fingerprintConfiguration);

        /// <summary>
        ///   Sets amender for default fingerprinting configuration
        /// </summary>
        /// <param name="amendFingerprintConfigFunctor">Amender</param>
        /// <returns>Query services selector</returns>
        IUsingQueryServices WithFingerprintConfig(Action<FingerprintConfiguration> amendFingerprintConfigFunctor);

        /// <summary>
        ///   Sets query configuration
        /// </summary>
        /// <param name="queryConfiguration">Query configuration to use</param>
        /// <returns>Query services selector</returns>
        IUsingQueryServices WithQueryConfig(QueryConfiguration queryConfiguration);

        /// <summary>
        ///  Sets amender for default query configuration
        /// </summary>
        /// <param name="amendQueryConfigFunctor">Amender</param>
        /// <returns>Query services selector</returns>
        IUsingQueryServices WithQueryConfig(Action<QueryConfiguration> amendQueryConfigFunctor);

        /// <summary>
        ///   Sets fingerprint and query configuration object
        /// </summary>
        /// <param name="fingerprintConfiguration">Fingerprint configuration to use</param>
        /// <param name="queryConfiguration">Query configuration to use</param>
        /// <returns>Query services selector</returns>
        IUsingQueryServices WithConfigs(FingerprintConfiguration fingerprintConfiguration, QueryConfiguration queryConfiguration);

        /// <summary>
        ///   Sets fingerprint and query amender of default fingerprint and query configurations
        /// </summary>
        /// <param name="amendFingerprintFunctor">Fingerprint configuration amender</param>
        /// <param name="amendQueryConfigFunctor">Query configuration amender</param>
        /// <returns>Query services selector</returns>
        IUsingQueryServices WithConfigs(Action<FingerprintConfiguration> amendFingerprintFunctor, Action<QueryConfiguration> amendQueryConfigFunctor);
    }
}