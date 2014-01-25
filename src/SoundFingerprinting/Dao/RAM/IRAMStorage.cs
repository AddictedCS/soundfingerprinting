namespace SoundFingerprinting.Dao.RAM
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal interface IRAMStorage
    {
        IDictionary<long, SubFingerprintData> SubFingerprints { get; }

        IDictionary<int, TrackData> Tracks { get; }

        IDictionary<int, IDictionary<long, HashData>> TracksHashes { get; }

        IDictionary<int, List<FingerprintData>> Fingerprints { get; }

        IDictionary<long, List<long>>[] HashTables { get; }

        int NumberOfHashTables { get; }

        void Reset(int numberOfHashTables);
    }
}