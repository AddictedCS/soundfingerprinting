namespace SoundFingerprinting.Dao.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public class RAMStorage
    {
        private static readonly RAMStorage Storage = new RAMStorage();

        private readonly ConcurrentDictionary<long, SubFingerprintData> subFingerprints;

        private readonly ConcurrentDictionary<int, TrackData> tracks;

        private readonly ConcurrentDictionary<int, ConcurrentDictionary<long, List<long>>> hashTables;

        private readonly ConcurrentDictionary<int, List<FingerprintData>> fingerprints;

        private RAMStorage()
        {
            subFingerprints = new ConcurrentDictionary<long, SubFingerprintData>();
            tracks = new ConcurrentDictionary<int, TrackData>();
            hashTables = new ConcurrentDictionary<int, ConcurrentDictionary<long, List<long>>>();
            fingerprints = new ConcurrentDictionary<int, List<FingerprintData>>();

            for (int table = 0; table < NumberOfHashTables; table++)
            {
                hashTables[table] = new ConcurrentDictionary<long, List<long>>();
            }
        }

        public static RAMStorage Instance
        {
            get
            {
                return Storage;
            }
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

        public int NumberOfHashTables { get; set; }
    }
}
