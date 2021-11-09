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
        void Add(AVHashes avHashes);

        bool Contains(DateTime captureTime);

        bool Contains(AVHashes fingerprints);

        AVHashes? Get(DateTime captureTime);

        void Remove(AVHashes fingerprints);
    }
}