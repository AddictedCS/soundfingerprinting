namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly IRAMStorage storage;
        private readonly IGroupingCounter groupingCounter;
        private readonly ISubFingerprintIdsToDataResolver storageResolver;

        public SubFingerprintDao(IRAMStorage storage, IGroupingCounter groupingCounter)
        {
            this.storage = storage;
            this.groupingCounter = groupingCounter;
            storageResolver = new RamStorageResolver(storage);
        }

        public int SubFingerprintsCount => storage.SubFingerprintsCount;

        public IEnumerable<int> HashCountsPerTable => storage.HashCountsPerTable;

        public IEnumerable<SubFingerprintData> InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            var subFingerprints = new List<SubFingerprintData>();
            foreach (var hashedFingerprint in hashes)
            {
                var subFingerprint = storage.AddHashedFingerprint(hashedFingerprint, trackReference);
                subFingerprints.Add(subFingerprint);
            }

            return subFingerprints;
        }

        public void InsertSubFingerprints(IEnumerable<SubFingerprintData> subFingerprints)
        {
            foreach (var subFingerprint in subFingerprints)
            {
                storage.AddSubFingerprint(subFingerprint);
            }
        }

        public IEnumerable<SubFingerprintData> ReadHashedFingerprintsByTrackReference(IModelReference trackReference)
        {
            return storage.ReadSubFingerprintByTrackReference(trackReference);
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, QueryConfiguration queryConfiguration)
        {
            var allSubs = new ConcurrentBag<SubFingerprintData>();
            int threshold = queryConfiguration.ThresholdVotes;
            var assignedClusters = queryConfiguration.Clusters;

            Parallel.ForEach(hashes, hashedFingerprint =>
            {
                var subFingerprints = QuerySubFingerprints(hashedFingerprint, threshold, assignedClusters);
                foreach (var subFingerprint in subFingerprints)
                {
                    allSubs.Add(subFingerprint);
                }
            });

            return new HashSet<SubFingerprintData>(allSubs);
        }

        public int DeleteSubFingerprintsByTrackReference(IModelReference trackReference)
        {
            return storage.DeleteSubFingerprintsByTrackReference(trackReference);
        }

        private IEnumerable<SubFingerprintData> QuerySubFingerprints(int[] hashes, int thresholdVotes, ISet<string> clusters)
        {
            var results = new List<uint>[hashes.Length];
            for (int table = 0; table < hashes.Length; ++table)
            {
                int hashBin = hashes[table];
                results[table] = storage.GetSubFingerprintsByHashTableAndHash(table, hashBin);
            }

            return groupingCounter.GroupByAndCount(results, thresholdVotes, clusters, storageResolver);
        }
    }
}
