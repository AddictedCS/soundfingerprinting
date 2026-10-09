namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Contract for realtime fingerprints interception.
    /// </summary>
    public interface IInterceptRealtimeFingerprints : IUsingRealtimeFingerprintServices
    {
        /// <summary>
        ///  Intercepts every <see cref="AVTrack"/> read from the realtime source before it is aggregated and fingerprinted.
        /// </summary>
        /// <param name="avTrackInterceptor">AVTrack interceptor.</param>
        /// <returns>Services selector.</returns>
        /// <remarks>
        ///  The track carries the stamp of its source: <see cref="StreamingFile.RelativeTo"/> for a streaming file, otherwise the stamp the producer set.
        ///  The interceptor is invoked for every track, also for one that is too short to yield hashes. The hashes take the <see cref="AVTrack.RelativeTo"/>
        ///  of the returned track, so an interceptor can re-stamp the stream, for example with a clock that runs on the decoded samples.
        /// </remarks>
        IInterceptRealtimeFingerprints InterceptAVTrack(Func<AVTrack, AVTrack> avTrackInterceptor);

        /// <summary>
        ///  Intercepts every <see cref="AVHashes"/> before it is emitted.
        /// </summary>
        /// <param name="hashesInterceptor">Hashes interceptor.</param>
        /// <returns>Services selector.</returns>
        IInterceptRealtimeFingerprints InterceptHashes(Func<AVHashes, AVHashes> hashesInterceptor);
    }
}
