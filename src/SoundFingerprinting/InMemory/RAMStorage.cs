namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;
    using Data;

    internal class RAMStorage : IRAMStorage
    {
        private IDictionary<IModelReference, SubFingerprintData> subFingerprints;

        private IDictionary<IModelReference, TrackData> tracks;

        private IDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>> tracksHashes;

        private IDictionary<long, List<IModelReference>>[] hashTables;

        private IDictionary<IModelReference, List<FingerprintData>> fingerprints;

        private IDictionary<IModelReference, List<SpectralImageData>> spectralImages;

        public RAMStorage(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        public IDictionary<IModelReference, SubFingerprintData> SubFingerprints => subFingerprints;

        public IDictionary<IModelReference, TrackData> Tracks => tracks;

        public IDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>> TracksHashes => tracksHashes;

        public IDictionary<IModelReference, List<FingerprintData>> Fingerprints => fingerprints;

        public IDictionary<IModelReference, List<SpectralImageData>> SpectralImages => spectralImages;

        public IDictionary<long, List<IModelReference>>[] HashTables => hashTables;

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
            spectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();

            for (int table = 0; table < numberOfHashTables; table++)
            {
                hashTables[table] = new Dictionary<long, List<IModelReference>>();
            }
        }
    }
}
