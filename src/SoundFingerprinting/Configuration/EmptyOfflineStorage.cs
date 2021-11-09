namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Empty offline storage.
    /// </summary>
    public class EmptyOfflineStorage : IOfflineStorage
    {
        /// <inheritdoc cref="IEnumerable{AVHashes}.GetEnumerator"/>
        public IEnumerator<AVHashes> GetEnumerator()
        {
            return Enumerable.Empty<AVHashes>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(AVHashes avHashes)
        {
            // no op
        }

        public bool Contains(DateTime captureTime)
        {
            return false;
        }

        public bool Contains(AVHashes fingerprints)
        {
            return false;
        }

        public AVHashes? Get(DateTime captureTime)
        {
            return null;
        }

        public void Remove(AVHashes fingerprints)
        {
            // no op
        }
    }
}