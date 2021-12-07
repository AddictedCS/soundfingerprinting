namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Contract for realtime source builder.
    /// </summary>
    public interface IRealtimeSource
    {
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
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<string> files, MediaType mediaType = MediaType.Audio);

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