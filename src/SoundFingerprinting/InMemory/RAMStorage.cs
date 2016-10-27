namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal class RAMStorage : IRAMStorage
    {
        private IDictionary<IModelReference, SubFingerprintData> subFingerprints;

        private IDictionary<IModelReference, TrackData> tracks;

        private IDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>> tracksHashes;

        private IDictionary<long, List<IModelReference>>[] hashTables;

        private IDictionary<IModelReference, List<FingerprintData>> fingerprints;

        public RAMStorage(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        public IDictionary<IModelReference, SubFingerprintData> SubFingerprints
        {
            get
            {
                return subFingerprints;
            }
        }

        public IDictionary<IModelReference, TrackData> Tracks
        {
            get
            {
                return tracks;
            }
        }

        public IDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>> TracksHashes
        {
            get
            {
                return tracksHashes;
            }
        }

        public IDictionary<IModelReference, List<FingerprintData>> Fingerprints
        {
            get
            {
                return fingerprints;
            }
        }

        public IDictionary<long, List<IModelReference>>[] HashTables
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
            subFingerprints = new Dictionary<IModelReference, SubFingerprintData>();
            tracks = new Dictionary<IModelReference, TrackData>();
            tracksHashes = new Dictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>>();
            hashTables = new Dictionary<long, List<IModelReference>>[NumberOfHashTables];
            fingerprints = new ConcurrentDictionary<IModelReference, List<FingerprintData>>();

            for (int table = 0; table < numberOfHashTables; table++)
            {
                hashTables[table] = new Dictionary<long, List<IModelReference>>();
            }
        }
    }
}
