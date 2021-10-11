namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    /// <summary>
    ///   Query configuration interface.
    /// </summary>
    public interface IWithQueryConfiguration : IUsingQueryServices
    {
        /// <summary>
        ///   Sets query configuration.
        /// </summary>
        /// <param name="queryConfiguration">Query configuration object to use.</param>
        /// <returns>Query services selector.</returns>
        IUsingQueryServices WithQueryConfig(QueryConfiguration queryConfiguration);

        /// <summary>
        ///   Sets query configuration parameters.
        /// </summary>
        /// <param name="amendQueryConfigFunctor">Amend functor.</param>
        /// <returns>Query services selector.</returns>
        IUsingQueryServices WithQueryConfig(Func<QueryConfiguration, QueryConfiguration> amendQueryConfigFunctor);
    }
}