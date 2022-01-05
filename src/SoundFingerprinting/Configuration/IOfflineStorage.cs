namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Offline storage for realtime hashes.
    /// </summary>
    public interface IOfflineStorage : IEnumerable<AVHashes>
    {
        /// <summary>
        ///  Adds an instance of <see cref="AVHashes"/> into the offline storage.
        /// </summary>
        /// <param name="avHashes">Audio/Video hashes to store.</param>
        void Add(AVHashes? avHashes);

        /// <summary>
        ///  Checks whether the offline storage contains hashes for this particular timestamp.
        /// </summary>
        /// <param name="captureTime">Capture time to check.</param>
        /// <returns>True - if offline storage contains hashes, otherwise false.</returns>
        bool Contains(DateTime captureTime);

        /// <summary>
        ///  Checks whether the current instance of the <see cref="AVHashes"/> is already in the offline storage.
        /// </summary>
        /// <param name="fingerprints">Instance of <see cref="AVHashes"/> to check.</param>
        /// <returns>True - if offline storage contains hashes, otherwise false.</returns>
        bool Contains(AVHashes fingerprints);

        /// <summary>
        ///  Returns an instance of <see cref="AVHashes"/> from offline storage or null of no hashes are available for current capture time.
        /// </summary>
        /// <param name="captureTime">Capture time to get hashes for.</param>
        /// <returns>An instance of <see cref="AVHashes"/> or null otherwise.</returns>
        AVHashes? Get(DateTime captureTime);

        /// <summary>
        ///  Remove an instance of <see cref="AVHashes"/> from the offline storage.
        /// </summary>
        /// <param name="fingerprints">Hashes to remove from the storage.</param>
        void Remove(AVHashes fingerprints);
    }
}