namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    /// <summary>
    ///   Configuration ammender interface
    /// </summary>
    public interface IWithQueryConfiguration : IUsingQueryServices
    {
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
        IUsingQueryServices WithQueryConfig(Func<QueryConfiguration, QueryConfiguration> amendQueryConfigFunctor);
    }
}