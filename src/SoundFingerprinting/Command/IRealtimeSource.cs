namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Contract for realtime source builder.
    /// </summary>
    public interface IRealtimeSource
    {
        /// <summary>
        ///  Build fingerprints from URL of a specific media type and length. 
        /// </summary>
        /// <param name="url">URL to the resource (i.e., broadcast stream).</param>
        /// <param name="chunkLength">Each AVTrack chunk emitted back via async enumerable will be of length provided by this parameter.</param>
        /// <param name="mediaType">Media type to extract from the URL.</param>
        /// <returns>Realtime query configuration selector.</returns>
        /// <remarks>
        ///  To build fingerprints directly from a realtime source, provide an instance of <see cref="IRealtimeMediaService"/> in <see cref="IUsingRealtimeQueryServices"/> interface.
        /// </remarks>
        IWithRealtimeQueryConfiguration From(string url, double chunkLength, MediaType mediaType = MediaType.Audio);
        
        /// <summary>
        ///  Build fingerprints from audio samples that are continuously coming from the realtime collection.
        /// </summary>
        /// <param name="source">Realtime collection to fetch audio samples from.</param>
        /// <returns>Realtime query configuration selector.</returns>
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> source);

        /// <summary>
        ///  Build fingerprints from the files that are continuously coming from the realtime collection.
        /// </summary>
        /// <param name="files">Realtime collection to fetch files from.</param>
        /// <param name="mediaType">Media type to fingerprint from provided files.</param>
        /// <returns>Realtime query configuration selector.</returns>
        /// <remarks>
        ///  If you want to build Video fingerprints from given files, provide an instance of <see cref="IMediaService"/> or <see cref="IVideoService"/> in <see cref="IUsingRealtimeQueryServices"/> interface.
        /// </remarks>
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<string> files, MediaType mediaType = MediaType.Audio);
        
        /// <summary>
        ///  Build fingerprints from streaming files that are continuously coming from the realtime collection.
        /// </summary>
        /// <param name="files">Realtime collection to fetch files from.</param>
        /// <returns>Realtime query configuration selector.</returns>
        /// <remarks>
        ///  Use this method whe you want to associate <see cref="Hashes.StreamId"/> and <see cref="Hashes.RelativeTo"/> with corresponding <see cref="StreamingFile"/>.
        /// </remarks>
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<StreamingFile> files);

        /// <summary>
        ///  Build fingerprints from <see cref="AVTrack"/>.
        /// </summary>
        /// <param name="tracks">Realtime collection to fetch AVTracks from.</param>
        /// <returns>Realtime query configuration selector.</returns>
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AVTrack> tracks);

        /// <summary>
        ///  Query source with pre-built fingerprints <see cref="AVHashes"/>.
        /// </summary>
        /// <param name="avHashes">Audio/Video hashes (instance of <see cref="AVHashes"/> class).</param>
        /// <returns>Realtime query configuration selector.</returns>
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AVHashes> avHashes);
    }
}