namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using SoundFingerprinting.Audio;

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
        /// <returns>Realtime query configuration selector.</returns>
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<string> files);
    }
}