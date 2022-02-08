namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Contract for realtime hashes interception.
    /// </summary>
    public interface IInterceptRealtimeSource : IUsingRealtimeQueryServices
    {
        /// <summary>
        ///  Intercept an instance of <see cref="AVTrack"/> class that was read from the realtime source.
        /// </summary>
        /// <param name="avTrackInterceptor">AVTrack interceptor.</param>
        /// <returns>Query services selector.</returns>
        /// <remarks>
        ///  Captures the result form the <see cref="IRealtimeMediaService.ReadAVTrackFromRealtimeSource"/> method invocation.
        /// </remarks>
        IInterceptRealtimeSource InterceptAVTrack(Func<AVTrack, AVTrack> avTrackInterceptor);

        /// <summary>
        ///  Intercept query hashes before they are used to query the data source.
        /// </summary>
        /// <param name="hashesInterceptor">Hashes interceptor that are used to query the storage.</param>
        /// <returns>Query services selector.</returns>
        IInterceptRealtimeSource InterceptHashes(Func<AVHashes, AVHashes> hashesInterceptor);

        /// <summary>
        ///  Intercepts query results, immediately after issuing a query.
        /// </summary>
        /// <param name="queryResultInterceptor">AVQueryResult interceptor.</param>
        /// <returns>Query services selector.</returns>
        IInterceptRealtimeSource InterceptQueryResults(Func<AVQueryResult, AVQueryResult> queryResultInterceptor);
    }
}