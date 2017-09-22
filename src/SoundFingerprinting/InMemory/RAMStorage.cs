namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using DAO;
    using DAO.Data;
    using Data;

    [Serializable]
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

        public void InitializeFromFile(string path)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                RAMStorage obj = (RAMStorage)formatter.Deserialize(stream);
                this.NumberOfHashTables = obj.NumberOfHashTables;
                this.SubFingerprints = obj.SubFingerprints;
                this.Tracks = obj.Tracks;
                this.TracksHashes = obj.TracksHashes;
                this.HashTables = obj.HashTables;
                this.Fingerprints = obj.Fingerprints;
                this.SpectralImages = obj.SpectralImages;
                stream.Close();
            }
        }

        public void Snapshot(string path)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, this);
                stream.Close();
            }
        }

        private void Initialize(int numberOfHashTables)
        {
            NumberOfHashTables = numberOfHashTables;
            SubFingerprints = new ConcurrentDictionary<IModelReference, SubFingerprintData>();
            Tracks = new ConcurrentDictionary<IModelReference, TrackData>();
            TracksHashes = new ConcurrentDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>>();
            HashTables = new ConcurrentDictionary<long, List<IModelReference>>[NumberOfHashTables];
            Fingerprints = new ConcurrentDictionary<IModelReference, List<FingerprintData>>();
            SpectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();

            for (int table = 0; table < numberOfHashTables; table++)
            {
                HashTables[table] = new ConcurrentDictionary<long, List<IModelReference>>();
            }
        }
    }
}
