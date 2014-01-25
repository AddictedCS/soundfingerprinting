namespace SoundFingerprinting.Dao.RAM
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal class RAMStorage : IRAMStorage
    {
        private IDictionary<long, SubFingerprintData> subFingerprints;

        private IDictionary<int, TrackData> tracks;

        private IDictionary<int, IDictionary<long, HashData>> tracksHashes;

        private IDictionary<long, List<long>>[] hashTables;

        private IDictionary<int, List<FingerprintData>> fingerprints;

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

        public IDictionary<int, IDictionary<long, HashData>> TracksHashes
        {
            get
            {
                return tracksHashes;
            }
        }

        public IDictionary<int, List<FingerprintData>> Fingerprints
        {
            get
            {
                return fingerprints;
            }
        }

        public IDictionary<long, List<long>>[] HashTables
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
            subFingerprints = new Dictionary<long, SubFingerprintData>();
            tracks = new Dictionary<int, TrackData>();
            tracksHashes = new Dictionary<int, IDictionary<long, HashData>>();
            hashTables = new Dictionary<long, List<long>>[NumberOfHashTables];
            fingerprints = new ConcurrentDictionary<int, List<FingerprintData>>();

            for (int table = 0; table < numberOfHashTables; table++)
            {
                hashTables[table] = new Dictionary<long, List<long>>();
            }
        }
    }
}
