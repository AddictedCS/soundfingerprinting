namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Configuration;

    public interface IWithRealtimeQueryConfiguration
    {
        /// <summary>
        ///  Sets realtime query configuration
        /// </summary>
        /// <param name="realtimeQueryConfiguration"></param>
        /// <returns>Query services selector</returns>
        IUsingRealtimeQueryServices WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration);

        /// <summary>
        ///  Sets realtime query configuration parameters
        /// </summary>
        /// <param name="amendQueryFunctor">Functor</param>
        /// <returns>Query services selector</returns>
        IUsingRealtimeQueryServices WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor);
    }
}