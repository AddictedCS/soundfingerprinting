namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Configuration;

    public interface IWithRealtimeQueryConfiguration : IInterceptRealtimeHashes
    {
        /// <summary>
        ///  Sets realtime query configuration
        /// </summary>
        /// <param name="realtimeQueryConfiguration"></param>
        /// <returns>Query services selector</returns>
        IInterceptRealtimeHashes WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration);

        /// <summary>
        ///  Sets realtime query configuration parameters
        /// </summary>
        /// <param name="amendQueryFunctor">Functor</param>
        /// <returns>Query services selector</returns>
        IInterceptRealtimeHashes WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor);
    }
}