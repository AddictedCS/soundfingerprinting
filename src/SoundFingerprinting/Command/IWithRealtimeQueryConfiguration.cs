namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Configuration;

    /// <summary>
    ///  Contract for realtime query configuration setter.
    /// </summary>
    public interface IWithRealtimeQueryConfiguration : IInterceptRealtimeSource
    {
        /// <summary>
        ///  Sets realtime query configuration.
        /// </summary>
        /// <param name="realtimeQueryConfiguration">Query configuration used for realtime query.</param>
        /// <returns>Query services selector.</returns>
        IInterceptRealtimeSource WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration);

        /// <summary>
        ///  Sets realtime query configuration parameters.
        /// </summary>
        /// <param name="amendQueryFunctor">Amend functor for and instance of <see cref="DefaultRealtimeQueryConfiguration"/>.</param>
        /// <returns>Query services selector.</returns>
        IInterceptRealtimeSource WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor);
    }
}