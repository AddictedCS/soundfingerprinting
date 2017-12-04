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

        [ProtoMember(6)]
        private long spectralImagesCounter;

        public IDictionary<ulong, SubFingerprintData> subFingerprints;

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

        public ConcurrentDictionary<int, List<ulong>>[] HashTables { get; set; }

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

        [ProtoMember(7)]
        private IDictionary<IModelReference, List<SpectralImageData>> SpectralImages { get; set; }

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

        public List<ulong> GetSubFingerprintsByHashTableAndHash(int table, int hash)
        {
            if (HashTables[table].TryGetValue(hash, out var subFingerprintIds))
            {
                return subFingerprintIds;
            }

            return Enumerable.Empty<ulong>().ToList();
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
                var obj = Serializer.Deserialize<RAMStorage>(file);
                trackReferenceCounter = obj.trackReferenceCounter;
                subFingerprintReferenceCounter = obj.subFingerprintReferenceCounter;
                NumberOfHashTables = obj.NumberOfHashTables;
                Tracks = obj.Tracks;
                SubFingerprints = obj.SubFingerprints;
                SpectralImages = obj.SpectralImages ?? new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();
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
            SpectralImages = new ConcurrentDictionary<IModelReference, List<SpectralImageData>>();
            SubFingerprints = new ConcurrentDictionary<ulong, SubFingerprintData>();
        }

        private void InitializeHashTablesIfNeedBe(int numberOfHashTables)
        {
            if (HashTables == null)
            {
                HashTables = new ConcurrentDictionary<int, List<ulong>>[numberOfHashTables];
                for (int table = 0; table < numberOfHashTables; table++)
                {
                    HashTables[table] = new ConcurrentDictionary<int, List<ulong>>();
                }
            }
        }

        private void InsertHashes(int[] hashBins, ulong subFingerprintId)
        {
            int table = 0;
            lock ((HashTables as ICollection).SyncRoot) // don't touch this lock
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
                        hashTable[hashBin] = new List<ulong> { subFingerprintId };
                    }

                    table++;
                }
            }
        }

        public void AddSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            int orderNumber = 0;
            var dtos = spectralImages.Select(spectralImage => new SpectralImageData(
                                spectralImage,
                                orderNumber++,
                                new ModelReference<ulong>((ulong)Interlocked.Increment(ref spectralImagesCounter)),
                                trackReference))
                            .ToList();

            lock (SpectralImages)
            {
                if (SpectralImages.TryGetValue(trackReference, out var existing))
                {
                    foreach (var dto in dtos)
                    {
                        existing.Add(dto);
                    }
                }
                else
                {
                    SpectralImages[trackReference] = dtos;
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
