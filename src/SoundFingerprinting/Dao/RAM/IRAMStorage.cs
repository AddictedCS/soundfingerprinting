namespace SoundFingerprinting.Dao.RAM
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal interface IRAMStorage
    {
        IDictionary<long, SubFingerprintData> SubFingerprints { get; }

        IDictionary<int, TrackData> Tracks { get; }

        IDictionary<int, List<FingerprintData>> Fingerprints { get; }

        IDictionary<int, ConcurrentDictionary<long, List<long>>> HashTables { get; }

        int NumberOfHashTables { get; }

        void Reset(int numberOfHashTables);
    }
}