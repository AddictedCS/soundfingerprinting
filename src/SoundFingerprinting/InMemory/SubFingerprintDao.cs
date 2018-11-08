namespace SoundFingerprinting.InMemory
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly IRAMStorage storage;
        private readonly IGroupingCounter groupingCounter;

        public SubFingerprintDao(IRAMStorage storage, IGroupingCounter groupingCounter)
        {
            this.storage = storage;
            this.groupingCounter = groupingCounter;
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
                var subFingerprints = ReadSubFingerprints(hashedFingerprint, threshold, assignedClusters);
                foreach (var subFingerprint in subFingerprints)
                {
                    allSubs.Add(subFingerprint);
                }
            });

            return allSubs;
        }

        public int DeleteSubFingerprintsByTrackReference(IModelReference trackReference)
        {
            return storage.DeleteSubFingerprintsByTrackReference(trackReference);
        }

        private IEnumerable<SubFingerprintData> ReadSubFingerprints(int[] hashes, int thresholdVotes, IEnumerable<string> assignedClusters)
        {
            var subFingerprints = CountSubFingerprintMatches(hashes, thresholdVotes);
            var clusters = assignedClusters as List<string> ?? assignedClusters.ToList();
            if (clusters.Any())
            {
                return subFingerprints.Where(subFingerprint => subFingerprint.Clusters.Intersect(clusters).Any());
            }

            return subFingerprints;
        }

        private IEnumerable<SubFingerprintData> CountSubFingerprintMatches(int[] hashes, int thresholdVotes)
        {
            var results = new IEnumerable<uint>[hashes.Length];
            for (int table = 0; table < hashes.Length; ++table)
            {
                int hashBin = hashes[table];
                results[table] = storage.GetSubFingerprintsByHashTableAndHash(table, hashBin);
            }

            return groupingCounter.GroupByAndCount(results, thresholdVotes, id => storage.ReadSubFingerprintById(id));
        }
    }
}