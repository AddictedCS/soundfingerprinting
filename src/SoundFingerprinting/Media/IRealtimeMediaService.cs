namespace SoundFingerprinting.Media
{
    using System.Collections.Generic;
    using System.Threading;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Media service that is able to read realtime chunks from provided URL.
    /// </summary>
    public interface IRealtimeMediaService
    {
        /// <summary>
        ///  Reads AVTrack from provided URL.
        /// </summary>
        /// <param name="url">URL to the resource (i.e., broadcast stream).</param>
        /// <param name="seconds">Each AVTrack chunk emitted back via async enumerable will be of length provided by this parameter.</param>
        /// <param name="avTrackReadConfiguration">An instance of <see cref="AVTrackReadConfiguration"/>.</param>
        /// <param name="mediaType">Media type to read.</param>
        /// <param name="cancellationToken">Cancellation token to cancel to stop reading from the source.</param>
        /// <returns>Async enumerable to iterate on.</returns>
        IAsyncEnumerable<AVTrack> ReadAVTrackFromRealtimeSource(string url, double seconds, AVTrackReadConfiguration avTrackReadConfiguration, MediaType mediaType, CancellationToken cancellationToken);
    }
}