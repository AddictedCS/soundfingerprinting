namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using SoundFingerprinting.Audio;

    public interface IRealtimeSource
    {
        /// <summary>
        ///  Build fingerprints from audio samples that are continuously coming from the realtime collection.
        /// </summary>
        /// <param name="realtimeCollection">Realtime collection to fetch audio samples from.</param>
        /// <returns>Realtime query configuration object.</returns>
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> realtimeCollection);

        /// <summary>
        ///  Build fingerprints from audio samples that are continuously coming from the realtime collection. 
        /// </summary>
        /// <param name="realtimeCollection">Realtime collection to fetch audio samples from.</param>
        /// <param name="streamId">Stream ID to associated with resulting hashes.</param>
        /// <returns>Realtime query configuration object.</returns>
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> realtimeCollection, string streamId);
    }
}