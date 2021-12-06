namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    /// <summary>
    ///   Query configuration interface.
    /// </summary>
    public interface IWithQueryConfiguration : IInterceptHashes
    {
        /// <summary>
        ///   Sets query configuration.
        /// </summary>
        /// <param name="queryConfiguration">Query configuration object to use.</param>
        /// <returns>Query services selector.</returns>
        IInterceptHashes WithQueryConfig(AVQueryConfiguration queryConfiguration);

        /// <summary>
        ///   Sets query configuration parameters.
        /// </summary>
        /// <param name="amendQueryConfigFunctor">Amend functor.</param>
        /// <returns>Query services selector.</returns>
        IInterceptHashes WithQueryConfig(Func<AVQueryConfiguration, AVQueryConfiguration> amendQueryConfigFunctor);
    }
}