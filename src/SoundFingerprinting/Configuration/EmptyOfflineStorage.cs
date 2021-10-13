namespace SoundFingerprinting.Configuration
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Empty offline storage.
    /// </summary>
    public class EmptyOfflineStorage : IOfflineStorage
    {
        /// <inheritdoc cref="IEnumerable{Hashes}.GetEnumerator"/>
        public IEnumerator<Hashes> GetEnumerator()
        {
            return Enumerable.Empty<Hashes>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}