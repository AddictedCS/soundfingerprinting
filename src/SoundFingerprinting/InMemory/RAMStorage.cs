namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DAO;
    using DAO.Data;
    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class RAMStorage : IRAMStorage
    {
        private ConcurrentDictionary<uint, SubFingerprintData> subFingerprints;

        public RAMStorage(int numberOfHashTables, string initializeFrom = "")
        {
            Initialize(numberOfHashTables);
            InitializeFromFile(initializeFrom);
        }

        private RAMStorage()
        {
            // left for proto-buf
        }

        public IEnumerable<int> HashCountsPerTable
        {
            get
            {
                return HashTables.Select(table => table.Count);
            }
        }

        [ProtoMember(3)] 
        private int NumberOfHashTables { get; set; }

        [ProtoMember(4)]
        public IDictionary<IModelReference, TrackData> Tracks { get; private set; }

        public int SubFingerprintsCount => subFingerprints.Count;

        public ConcurrentDictionary<int, List<uint>>[] HashTables { get; set; }

        [ProtoMember(5)]
        private ConcurrentDictionary<uint, SubFingerprintData> SubFingerprints
        {
            get => subFingerprints;

            set
            {
                if (value == null)
                {
                    return;
                }

                subFingerprints = value;
                InitializeHashTablesIfNeedBe(NumberOfHashTables);
                foreach (var pair in value)
                {
                    InsertHashes(pair.Value.Hashes, pair.Key);
                }
            }
        }

        [ProtoMember(7)]
        private IDictionary<IModelReference, List<SpectralImageData>> SpectralImages { get; set; }

        public void AddSubFingerprint(SubFingerprintData subFingerprintData)
        {
            SubFingerprints[(uint)subFingerprintData.SubFingerprintReference.Id] = subFingerprintData;
            InsertHashes(subFingerprintData.Hashes, (uint)subFingerprintData.SubFingerprintReference.Id);
        }

        public TrackData AddTrack(TrackData track)
        {
            return Tracks[track.TrackReference] = track;
        }

        public int DeleteSubFingerprintsByTrackReference(IModelReference trackReference)
        {
            var all = from @ref in SubFingerprints.Values
                where @ref.TrackReference.Equals(trackReference)
                select (uint)@ref.SubFingerprintReference.Id;

            var references = new HashSet<uint>(all);

            lock ((HashTables as ICollection).SyncRoot)
            {
                foreach (var reference in references)
                {
                    SubFingerprints.TryRemove(reference, out _);
                }

                int totals = HashTables.AsParallel().Aggregate(0, (removed, hashTable) =>
                {
                    return removed + hashTable.Values.Aggregate(0,
                               (accumulator, list) =>
                               {
                                   return accumulator + list.RemoveAll(id => references.Contains(id));
                               });
                });
                
                return totals + references.Count;
            }
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            if (Tracks.Remove(trackReference))
            {
                return 1;
            }

            return 0;
        }

        public List<uint> GetSubFingerprintsByHashTableAndHash(int table, int hash)
        {
            if (HashTables[table].TryGetValue(hash, out var subFingerprintIds))
            {
                return subFingerprintIds;
            }

            return Enumerable.Empty<uint>().ToList();
        }

        public SubFingerprintData ReadSubFingerprintById(uint id)
        {
            return SubFingerprints[id];
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintByTrackReference(IModelReference trackReference)
        {
            return SubFingerprints.Where(pair => pair.Value.TrackReference.Equals(trackReference)).Select(pair => pair.Value).ToList();
        }

        public void Reset(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        private void InitializeFromFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            
            using var file = File.OpenRead(path);
            var obj = Serializer.Deserialize<RAMStorage>(file);
            NumberOfHashTables = obj.NumberOfHashTables;
            Tracks = obj.Tracks;
            SubFingerprints = obj.SubFingerprints;
            SpectralImages = obj.SpectralImages ?? new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();
        }

        public void Snapshot(string path)
        {
            using var file = File.Create(path);
            Serializer.Serialize(file, this);
        }

        private void Initialize(int numberOfHashTables)
        {
            NumberOfHashTables = numberOfHashTables;
            Tracks = new ConcurrentDictionary<IModelReference, TrackData>();
            SpectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();
            SubFingerprints = new ConcurrentDictionary<uint, SubFingerprintData>();
        }

        private void InitializeHashTablesIfNeedBe(int numberOfHashTables)
        {
            if (HashTables == null)
            {
                HashTables = new ConcurrentDictionary<int, List<uint>>[numberOfHashTables];
                for (int table = 0; table < numberOfHashTables; table++)
                {
                    HashTables[table] = new ConcurrentDictionary<int, List<uint>>();
                }
            }
        }

        private void InsertHashes(int[] hashBins, uint subFingerprintId)
        {
            int table = 0;
            lock ((HashTables as ICollection).SyncRoot)
            {
                foreach (var hashBin in hashBins)
                {
                    var hashTable = HashTables[table];

                    if (hashTable.TryGetValue(hashBin, out var subFingerprintsList))
                    {
                        subFingerprintsList.Add(subFingerprintId);
                    }
                    else
                    {
                        hashTable[hashBin] = new List<uint> { subFingerprintId };
                    }

                    table++;
                }
            }
        }

        public void AddSpectralImages(IEnumerable<SpectralImageData> spectralImages)
        {
            var images = spectralImages.ToList();
            var trackReference = images.First().TrackReference;
            lock (SpectralImages)
            {
                if (SpectralImages.TryGetValue(trackReference, out var existing))
                {
                    foreach (var dto in images)
                    {
                        existing.Add(dto);
                    }
                }
                else
                {
                    SpectralImages[trackReference] = images;
                }
            }
        }

        public IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference)
        {
            if (SpectralImages.TryGetValue(trackReference, out var spectralImageDatas))
            {
                return spectralImageDatas;
            }

            return Enumerable.Empty<SpectralImageData>().ToList();
        }
    }
}
