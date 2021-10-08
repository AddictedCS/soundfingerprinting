namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Configuration;

    /// <summary>
    ///  Contract for realtime query configuration setter.
    /// </summary>
    public interface IWithRealtimeQueryConfiguration : IInterceptRealtimeHashes
    {
        /// <summary>
        ///  Sets realtime query configuration.
        /// </summary>
        /// <param name="realtimeQueryConfiguration">Query configuration used for realtime query.</param>
        /// <returns>Query services selector.</returns>
        IInterceptRealtimeHashes WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration);

        /// <summary>
        ///  Sets realtime query configuration parameters.
        /// </summary>
        /// <param name="amendQueryFunctor">Amend functor for and instance of <see cref="RealtimeQueryConfiguration"/>.</param>
        /// <returns>Query services selector.</returns>
        IInterceptRealtimeHashes WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor);
    }
}