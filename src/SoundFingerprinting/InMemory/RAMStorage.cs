namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DAO;
    using DAO.Data;
    using Data;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    internal class RAMStorage : IRAMStorage
    {
        [NonSerialized]
        private IDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>> tracksHashes;

        [NonSerialized]
        private IDictionary<IModelReference, List<FingerprintData>> fingerprints;

        [NonSerialized]
        private IDictionary<IModelReference, List<SpectralImageData>> spectralImages;

        public RAMStorage()
        {
            
        }

        public RAMStorage(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        public IDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>> TracksHashes
        {
            get
            {
                return tracksHashes;
            }
            private set
            {
                tracksHashes = value;
            }
        }

        public IDictionary<IModelReference, List<FingerprintData>> Fingerprints
        {
            get
            {
                return fingerprints;
            }
            private set
            {
                fingerprints = value;
            }
        }

        public IDictionary<IModelReference, List<SpectralImageData>> SpectralImages
        {
            get
            {
                return spectralImages;
            }
            private set
            {
                spectralImages = value;
            }
        }

        [ProtoMember(1)]
        public IDictionary<int, TrackData> Tracks { get; private set; }

        public IDictionary<long, List<ulong>>[] HashTables { get; private set; }

        [ProtoMember(2)]
        private Table[] InnerHashTables
        {
            get
            {
                if (HashTables == null)
                {
                    return new Table[0];
                }

                return HashTables.Select(
                    (table, index) =>
                    {
                        var hashes = table.Select(pair => new HashedBin { Hash = pair.Key, References = pair.Value }).ToArray();
                        return new Table { HashedBins = hashes };
                    }).ToArray();
            }
            set
            {
                HashTables = new IDictionary<long, List<ulong>>[value.Length];
                for (int index = 0; index < value.Length; ++index)
                {
                    HashTables[index] = new ConcurrentDictionary<long, List<ulong>>();
                    foreach (var hashBin in value[index].HashedBins)
                    {
                        HashTables[index].Add(hashBin.Hash, hashBin.References);
                    }
                }
            }
        }

        [ProtoMember(3)]
        public IDictionary<ulong, SubFingerprintData> SubFingerprints { get; private set; }

        [ProtoMember(4)]
        public int NumberOfHashTables { get; private set; }

        public void Reset(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        public void InitializeFromFile(string path)
        {
            using (var file = File.OpenRead(path))
            {
                RAMStorage obj = Serializer.Deserialize<RAMStorage>(file);
                NumberOfHashTables = obj.NumberOfHashTables;
                Tracks = obj.Tracks;
                HashTables = obj.HashTables;
                SubFingerprints = obj.SubFingerprints;

                Fingerprints = new ConcurrentDictionary<IModelReference, List<FingerprintData>>();
                SpectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();
                TracksHashes = new ConcurrentDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>>();
            }
        }

        public void Snapshot(string path)
        {
            using (var file = File.Create(path))
            {
                Serializer.Serialize(file, this);
            }
        }

        private void Initialize(int numberOfHashTables)
        {
            NumberOfHashTables = numberOfHashTables;
            SubFingerprints = new ConcurrentDictionary<ulong, SubFingerprintData>();
            Tracks = new ConcurrentDictionary<int, TrackData>();
            TracksHashes = new ConcurrentDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>>();
            HashTables = new ConcurrentDictionary<long, List<ulong>>[NumberOfHashTables];
            Fingerprints = new ConcurrentDictionary<IModelReference, List<FingerprintData>>();
            SpectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();

            for (int table = 0; table < numberOfHashTables; table++)
            {
                HashTables[table] = new ConcurrentDictionary<long, List<ulong>>();
            }
        }
    }

    [ProtoContract]
    internal class HashedBin   // The intermediate type
    {
        [ProtoMember(1)]
        public long Hash { get; set; }

        [ProtoMember(2)]
        public List<ulong> References { get; set; }

    }

    [ProtoContract]
    internal class Table
    {
        [ProtoMember(1)]
        public HashedBin[] HashedBins { get; set; }
    }
}
