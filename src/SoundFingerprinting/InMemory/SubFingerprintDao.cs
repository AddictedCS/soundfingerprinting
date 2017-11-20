namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Utils;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly IRAMStorage storage;

        public SubFingerprintDao(IRAMStorage storage)
        {
            this.storage = storage;
        }

        public void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            foreach (var hashedFingerprint in hashes)
            {
                storage.AddSubfingerprint(hashedFingerprint, trackReference);
            }
        }

        public IList<HashedFingerprint> ReadHashedFingerprintsByTrackReference(IModelReference trackReference)
        {
            var subFingerprints = storage.ReadSubFingerprintByTrackReference(trackReference);

            return subFingerprints.Select(data => new HashedFingerprint(
                    data.Hashes,
                    data.SequenceNumber,
                    data.SequenceAt,
                    data.Clusters))
                .ToList();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprints(int[] hashes, int thresholdVotes, IEnumerable<string> assignedClusters)
        {
            var subFingeprintCount = CountSubFingerprintMatches(hashes, thresholdVotes);
            var subFingerprints = subFingeprintCount.Select(id => storage.ReadSubFingerprintById(id));

            var clusters = assignedClusters as List<string> ?? assignedClusters.ToList();
            if (clusters.Any())
            {
                return subFingerprints.Where(subFingerprint => subFingerprint.Clusters.Intersect(clusters).Any());
            }

            return subFingerprints;
        }

        public ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, int threshold, IEnumerable<string> assignedClusters)
        {
            var allSubs = new ConcurrentBag<SubFingerprintData>();
            Parallel.ForEach(hashes, hashedFingerprint =>
            {
                var subFingerprints = ReadSubFingerprints(hashedFingerprint, threshold, assignedClusters);
                foreach (var subFingerprint in subFingerprints)
                {
                    allSubs.Add(subFingerprint);
                }
            });

            return new HashSet<SubFingerprintData>(allSubs);
        }

        private IEnumerable<ulong> CountSubFingerprintMatches(int[] hashes, int thresholdVotes)
        {
            var results = new List<ulong>[hashes.Length];
            for (int table = 0; table < hashes.Length; ++table)
            {
                int hashBin = hashes[table];
                results[table] = storage.GetSubFingerprintsByHashTableAndHash(table, hashBin);
            }

            return SubFingerprintGroupingCounter.GroupByAndCount(results, thresholdVotes);
        }
    }
}
