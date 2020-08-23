namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
            get { return HashTables.Select(table => table.Count); }
        }

        [ProtoMember(3)] private int NumberOfHashTables { get; set; }

        [ProtoMember(4)] public IDictionary<IModelReference, TrackData> Tracks { get; private set; }

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

        [ProtoMember(7)] private IDictionary<IModelReference, List<SpectralImageData>> SpectralImages { get; set; }

        public void AddSubFingerprint(SubFingerprintData subFingerprintData)
        {
            SubFingerprints[subFingerprintData.SubFingerprintReference.Get<uint>()] = subFingerprintData;
            InsertHashes(subFingerprintData.Hashes, subFingerprintData.SubFingerprintReference.Get<uint>());
        }

        public TrackData AddTrack(TrackData track)
        {
            return Tracks[track.TrackReference] = track;
        }

        public int DeleteSubFingerprintsByTrackReference(IModelReference trackReference)
        {
            var all = ReadSubFingerprintByTrackReference(trackReference).ToList();
            foreach (var reference in all)
            {
                SubFingerprints.TryRemove(reference.SubFingerprintReference.Get<uint>(), out _);
            }

            int totals = 0;
            Parallel.ForEach(all, subFingerprintData =>
            {
                int[] hashes = subFingerprintData.Hashes;
                for (int table = 0; table < hashes.Length; ++table)
                {
                    if (HashTables[table].TryGetValue(hashes[table], out var list))
                    {
                        lock (list)
                        {
                            int removed = list.RemoveAll(id => id == subFingerprintData.SubFingerprintReference.Get<uint>());
                            Interlocked.Add(ref totals, removed);
                        }
                    }
                }
            });

            return totals + all.Count;
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
            foreach (var hashBin in hashBins)
            {
                var hashTable = HashTables[table];
                var ids = hashTable.GetOrAdd(hashBin, _ => new List<uint>());
                lock (ids)
                {
                    ids.Add(subFingerprintId);
                }

                table++;
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