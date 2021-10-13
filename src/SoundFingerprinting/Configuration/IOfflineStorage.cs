namespace SoundFingerprinting.Configuration
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Offline storage for realtime hashes.
    /// </summary>
    public interface IOfflineStorage : IEnumerable<Hashes>
    {
        // no op, reserved for future use
    }
}