namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;
    using Data;

    internal class RAMStorage : IRAMStorage
    {
        public RAMStorage(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        public IDictionary<IModelReference, SubFingerprintData> SubFingerprints { get; private set; }

        public IDictionary<IModelReference, TrackData> Tracks { get; private set; }

        public IDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>> TracksHashes { get; private set; }

        public IDictionary<IModelReference, List<FingerprintData>> Fingerprints { get; private set; }

        public IDictionary<IModelReference, List<SpectralImageData>> SpectralImages { get; private set; }

        public IDictionary<long, List<IModelReference>>[] HashTables { get; private set; }

        public int NumberOfHashTables { get; private set; }

        public void Reset(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        private void Initialize(int numberOfHashTables)
        {
            NumberOfHashTables = numberOfHashTables;
            SubFingerprints = new Dictionary<IModelReference, SubFingerprintData>();
            Tracks = new Dictionary<IModelReference, TrackData>();
            TracksHashes = new Dictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>>();
            HashTables = new Dictionary<long, List<IModelReference>>[NumberOfHashTables];
            Fingerprints = new ConcurrentDictionary<IModelReference, List<FingerprintData>>();
            SpectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();

            for (int table = 0; table < numberOfHashTables; table++)
            {
                HashTables[table] = new Dictionary<long, List<IModelReference>>();
            }
        }
    }
}
