namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Contract for realtime fingerprint source builder.
    /// </summary>
    /// <remarks>
    ///  The enumerable returned by <see cref="IRealtimeFingerprintCommand.Hash"/> re-enumerates the provided source every time it is enumerated.
    ///  Sources passed to any of the <c>From</c> overloads have to be hot (i.e., emitting live data, resuming from where they left off when re-enumerated).
    /// </remarks>
    public interface IRealtimeFingerprintSource
    {
        /// <summary>
        ///  Build fingerprints from URL of a specific media type and length.
        /// </summary>
        /// <param name="url">URL to the resource (i.e., broadcast stream).</param>
        /// <param name="chunkLength">Each AVTrack chunk emitted back via async enumerable will be of length provided by this parameter.</param>
        /// <param name="mediaType">Media type to extract from the URL.</param>
        /// <returns>Realtime fingerprint configuration selector.</returns>
        /// <remarks>
        ///  Provide an instance of <see cref="IRealtimeMediaService"/> in <see cref="IUsingRealtimeFingerprintServices"/> to build fingerprints directly from a URL.
        /// </remarks>
        IWithRealtimeFingerprintConfiguration From(string url, double chunkLength, MediaType mediaType = MediaType.Audio);

        /// <summary>
        ///  Build fingerprints from audio samples that are continuously coming from the realtime collection.
        /// </summary>
        /// <param name="source">Realtime collection to fetch audio samples from.</param>
        /// <returns>Realtime fingerprint configuration selector.</returns>
        IWithRealtimeFingerprintConfiguration From(IAsyncEnumerable<AudioSamples> source);

        /// <summary>
        ///  Build fingerprints from the files that are continuously coming from the realtime collection.
        /// </summary>
        /// <param name="files">Realtime collection to fetch files from.</param>
        /// <param name="mediaType">Media type to fingerprint from provided files.</param>
        /// <returns>Realtime fingerprint configuration selector.</returns>
        /// <remarks>
        ///  To build video fingerprints from the files, provide an instance of <see cref="IMediaService"/> or <see cref="IVideoService"/> in <see cref="IUsingRealtimeFingerprintServices"/>.
        /// </remarks>
        IWithRealtimeFingerprintConfiguration From(IAsyncEnumerable<string> files, MediaType mediaType = MediaType.Audio);

        /// <summary>
        ///  Build fingerprints from streaming files that are continuously coming from the realtime collection.
        /// </summary>
        /// <param name="files">Realtime collection to fetch files from.</param>
        /// <returns>Realtime fingerprint configuration selector.</returns>
        /// <remarks>
        ///  <see cref="StreamingFile.RelativeTo"/> is the capture stamp of the file and <see cref="StreamingFile.StreamId"/> is set on the resulting hashes.
        /// </remarks>
        IWithRealtimeFingerprintConfiguration From(IAsyncEnumerable<StreamingFile> files);

        /// <summary>
        ///  Build fingerprints from audio/video tracks that are continuously coming from the realtime collection.
        /// </summary>
        /// <param name="tracks">Realtime collection to fetch tracks from.</param>
        /// <returns>Realtime fingerprint configuration selector.</returns>
        IWithRealtimeFingerprintConfiguration From(IAsyncEnumerable<AVTrack> tracks);
    }
}
