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

        public void Insert(long[] hashBins, long subFingerprintId)
        {
            int table = 0;
            lock (((ICollection)storage.HashTables).SyncRoot)
            {
                foreach (var hashTable in storage.HashTables)
                {
                    if (!hashTable.ContainsKey(hashBins[table]))
                    {
                        hashTable[hashBins[table]] = new List<long>();
                    }

                    hashTable[hashBins[table]].Add(subFingerprintId);
                    table++;
                }

                int trackId = ((ModelReference<int>)storage.SubFingerprints[subFingerprintId].TrackReference).Id;
                storage.TracksHashes[trackId][subFingerprintId].HashBins = hashBins;
            }
        }

        public IList<HashBinData> ReadHashBinsByHashTable(int hashTableId)
        {
            if (storage.HashTables.Length < hashTableId)
            {
                return Enumerable.Empty<HashBinData>().ToList();
            }

            var hashTable = storage.HashTables[hashTableId - 1];
            List<HashBinData> hashBins = new List<HashBinData>();
            foreach (var hashBinPair in hashTable)
            {
                foreach (var subFingerprintId in hashBinPair.Value)
                {
                    HashBinData hashBin = new HashBinData
                        {
                            HashTable = hashTableId,
                            HashBin = hashBinPair.Key,
                            SubFingerprintReference = new ModelReference<long>(subFingerprintId)
                        };
                    hashBins.Add(hashBin);
                }
            }

            return hashBins;
        }

        public IList<HashData> ReadHashDataByTrackId(int trackId)
        {
            return storage.TracksHashes[trackId].Values.ToList();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(
            long[] hashBuckets, int thresholdVotes)
        {
            int table = 0;
            var hashTables = storage.HashTables;
            Dictionary<long, int> subFingeprintCount = new Dictionary<long, int>();
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
            var trackIds = storage.Tracks.Where(pair => pair.Value.GroupId == trackGroupId)
                                         .Select(pair => pair.Value.TrackReference).ToList();
            
            if (trackIds.Any())
            {
                return ReadSubFingerprintDataByHashBucketsWithThreshold(hashBuckets, thresholdVotes)
                    .Where(subFingerprint => trackIds.Contains(subFingerprint.TrackReference));
            }

            return Enumerable.Empty<SubFingerprintData>();
        }
    }
}
