namespace SoundFingerprinting.Dao.RAM
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class HashBinDao : IHashBinDao
    {
        private readonly IRAMStorage storage;

        public HashBinDao()
            : this(DependencyResolver.Current.Get<IRAMStorage>())
        {
            // no op
        }

        public HashBinDao(IRAMStorage storage)
        {
            this.storage = storage;
        }

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference)
        {
            int table = 0;
            lock (((ICollection)storage.HashTables).SyncRoot)
            {
                foreach (var hashTable in storage.HashTables)
                {
                    if (!hashTable.ContainsKey(hashBins[table]))
                    {
                        hashTable[hashBins[table]] = new List<IModelReference>();
                    }

                    hashTable[hashBins[table]].Add(subFingerprintReference);
                    table++;
                }

                var trackReference = storage.SubFingerprints[subFingerprintReference].TrackReference;
                storage.TracksHashes[trackReference][subFingerprintReference].HashBins = hashBins;
            }
        }

        public IList<HashBinData> ReadHashBinsByHashTable(int hashTableId)
        {
            if (storage.HashTables.Length < hashTableId)
            {
                return Enumerable.Empty<HashBinData>().ToList();
            }

            var hashTable = storage.HashTables[hashTableId - 1];
            var hashBins = new List<HashBinData>();
            foreach (var hashBinPair in hashTable)
            {
                foreach (var subFingerprintId in hashBinPair.Value)
                {
                    HashBinData hashBin = new HashBinData
                        {
                            HashTable = hashTableId,
                            HashBin = hashBinPair.Key,
                            SubFingerprintReference = subFingerprintId
                        };
                    hashBins.Add(hashBin);
                }
            }

            return hashBins;
        }

        public IList<HashData> ReadHashDataByTrackId(IModelReference trackReference)
        {
            return storage.TracksHashes[trackReference].Values.ToList();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(
            long[] hashBuckets, int thresholdVotes)
        {
            int table = 0;
            var hashTables = storage.HashTables;
            var subFingeprintCount = new Dictionary<IModelReference, int>();
            foreach (var hashBin in hashBuckets)
            {
                if (hashTables[table].ContainsKey(hashBin))
                {
                    foreach (var subFingerprintId in hashTables[table][hashBin])
                    {
                        if (!subFingeprintCount.ContainsKey(subFingerprintId))
                        {
                            subFingeprintCount[subFingerprintId] = 0;
                        }

                        subFingeprintCount[subFingerprintId]++;
                    }
                }

                table++;
            }

            return subFingeprintCount.Where(pair => pair.Value >= thresholdVotes)
                                     .Select(pair => storage.SubFingerprints[pair.Key]);
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] hashBuckets, int thresholdVotes, string trackGroupId)
        {
            var trackReferences = storage.Tracks.Where(pair => pair.Value.GroupId == trackGroupId)
                                         .Select(pair => pair.Value.TrackReference).ToList();
            
            if (trackReferences.Any())
            {
                return ReadSubFingerprintDataByHashBucketsWithThreshold(hashBuckets, thresholdVotes)
                    .Where(subFingerprint => trackReferences.Contains(subFingerprint.TrackReference));
            }

            return Enumerable.Empty<SubFingerprintData>();
        }
    }
}
