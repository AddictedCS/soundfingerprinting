namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using DAO;
    using DAO.Data;
    using Data;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    internal class RAMStorage : IRAMStorage
    {
        [ProtoMember(1)]
        private long subFingerprintReferenceCounter;

        [ProtoMember(2)]
        private int trackReferenceCounter;

        private IDictionary<ulong, SubFingerprintData> subFingerprints;

        public RAMStorage()
        {
            // no op
        }

        public RAMStorage(int numberOfHashTables)
        {
            Initialize(numberOfHashTables);
        }

        [ProtoMember(3)]
        public int NumberOfHashTables { get; private set; }

        [ProtoMember(4)]
        public IDictionary<int, TrackData> Tracks { get; private set; }

        private ConcurrentDictionary<long, List<ulong>>[] HashTables { get; set; }

        [ProtoMember(5)]
        private IDictionary<ulong, SubFingerprintData> SubFingerprints
        {
            get
            {
                return subFingerprints;
            }
            set
            {
                if (value == null) return;

                subFingerprints = value;
                InitializeHashTablesIfNeedBe(NumberOfHashTables);
                foreach (var pair in value)
                {
                    InsertHashes(pair.Value.Hashes, pair.Key);
                }
            }
        }

        public IDictionary<IModelReference, List<FingerprintData>> Fingerprints { get; private set; }

        public IDictionary<IModelReference, List<SpectralImageData>> SpectralImages { get; private set; }

        public void AddSubfingerprint(HashedFingerprint hashedFingerprint, IModelReference trackReference)
        {
            var subFingerprintReference = new ModelReference<ulong>((ulong)Interlocked.Increment(ref subFingerprintReferenceCounter));
            var subFingerprintData = new SubFingerprintData(
                                             hashedFingerprint.HashBins,
                                             hashedFingerprint.SequenceNumber,
                                             hashedFingerprint.StartsAt,
                                             subFingerprintReference,
                                             trackReference)
                                         {
                                             Clusters = hashedFingerprint.Clusters
                                         };

            SubFingerprints[(ulong)subFingerprintData.SubFingerprintReference.Id] = subFingerprintData;
            InsertHashes(hashedFingerprint.HashBins, subFingerprintReference.Id);
        }

        public IModelReference AddTrack(TrackData track)
        {
            var trackReference = new ModelReference<int>(Interlocked.Increment(ref trackReferenceCounter));
            Tracks[trackReference.Id] = track;
            return track.TrackReference = trackReference;
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            int count = 0;
            int trackId = (int)trackReference.Id;
            if (Tracks.Remove(trackId))
            {
                count++;
                if (Fingerprints.ContainsKey(trackReference))
                {
                    count += Fingerprints[trackReference].Count;
                    Fingerprints.Remove(trackReference);
                }

                var subFingerprintReferences = SubFingerprints
                    .Where(pair => pair.Value.TrackReference.Equals(trackReference)).Select(pair => pair.Key).ToList();

                count += subFingerprintReferences.Count;
                foreach (var subFingerprintReference in subFingerprintReferences)
                {
                    SubFingerprints.Remove(subFingerprintReference);
                }

                foreach (var hashTable in HashTables)
                {
                    foreach (var hashBins in hashTable)
                    {
                        foreach (var subFingerprintReference in subFingerprintReferences)
                        {
                            if (hashBins.Value.Remove(subFingerprintReference))
                            {
                                count++;
                            }
                        }
                    }
                }
            }

            return count;
        }

        public IEnumerable<ulong> GetSubFingerprintsByHashTableAndHash(int table, long hash)
        {
            return HashTables[table].ContainsKey(hash) ? HashTables[table][hash] : Enumerable.Empty<ulong>();
        }

        public SubFingerprintData ReadSubFingerprintById(ulong id)
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

        public void InitializeFromFile(string path)
        {
            using (var file = File.OpenRead(path))
            {
                RAMStorage obj = Serializer.Deserialize<RAMStorage>(file);
                trackReferenceCounter = obj.trackReferenceCounter;
                subFingerprintReferenceCounter = obj.subFingerprintReferenceCounter;
                NumberOfHashTables = obj.NumberOfHashTables;
                Tracks = obj.Tracks;
                SubFingerprints = obj.SubFingerprints;

                Fingerprints = new ConcurrentDictionary<IModelReference, List<FingerprintData>>();
                SpectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();
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
            trackReferenceCounter = 0;
            subFingerprintReferenceCounter = 0;
            Tracks = new ConcurrentDictionary<int, TrackData>();
            Fingerprints = new ConcurrentDictionary<IModelReference, List<FingerprintData>>();
            SpectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();
            SubFingerprints = new ConcurrentDictionary<ulong, SubFingerprintData>();
        }

        private void InitializeHashTablesIfNeedBe(int numberOfHashTables)
        {
            lock (this)
            {
                if (HashTables == null)
                {
                    HashTables = new ConcurrentDictionary<long, List<ulong>>[numberOfHashTables];
                    for (int table = 0; table < numberOfHashTables; table++)
                    {
                        HashTables[table] = new ConcurrentDictionary<long, List<ulong>>();
                    }
                }
            }
        }

        private void InsertHashes(long[] hashBins, ulong subFingerprintId)
        {
            for (int table = 0; table < HashTables.Length; ++table)
            {
                var hashTable = HashTables[table];
                long key = hashBins[table];
                hashTable.AddOrUpdate(
                    key,
                    new List<ulong>(),
                    (keyTo, list) =>
                    {
                        list.Add(subFingerprintId);
                        return list;
                    });
            }
        }
    }
}
