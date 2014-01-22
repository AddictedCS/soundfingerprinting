namespace SoundFingerprinting.Dao.RAM
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal class RAMStorage : IRAMStorage
    {
        private ConcurrentDictionary<long, SubFingerprintData> subFingerprints;

        private ConcurrentDictionary<int, TrackData> tracks;

        private ConcurrentDictionary<int, ConcurrentDictionary<long, List<long>>> hashTables;

        private ConcurrentDictionary<int, List<FingerprintData>> fingerprints;

        public RAMStorage(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        public IDictionary<long, SubFingerprintData> SubFingerprints
        {
            get
            {
                return subFingerprints;
            }
        }

        public IDictionary<int, TrackData> Tracks
        {
            get
            {
                return tracks;
            }
        }

        public IDictionary<int, List<FingerprintData>> Fingerprints
        {
            get
            {
                return fingerprints;
            }
        }

        public IDictionary<int, ConcurrentDictionary<long, List<long>>> HashTables
        {
            get
            {
                return hashTables;
            }
        }

        public int NumberOfHashTables { get; private set; }

        public void Reset(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        private void Initialize(int numberOfHashTables)
        {
            NumberOfHashTables = numberOfHashTables;
            subFingerprints = new ConcurrentDictionary<long, SubFingerprintData>();
            tracks = new ConcurrentDictionary<int, TrackData>();
            hashTables = new ConcurrentDictionary<int, ConcurrentDictionary<long, List<long>>>();
            fingerprints = new ConcurrentDictionary<int, List<FingerprintData>>();

            for (int table = 1; table <= numberOfHashTables; table++)
            {
                hashTables[table] = new ConcurrentDictionary<long, List<long>>();
            }
        }
    }
}
