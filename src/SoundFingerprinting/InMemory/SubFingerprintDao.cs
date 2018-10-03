namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
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

        public SubFingerprintDao(IRAMStorage storage, IGroupingCounter groupingCounter)
        {
            this.storage = storage;
            this.groupingCounter = groupingCounter;
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
            return subFingerprints.Select(data => new HashedFingerprint(data.Hashes, data.SequenceNumber, data.SequenceAt, data.Clusters)).ToList();
        }

        public FingerprintsQueryResponse ReadSubFingerprints(IEnumerable<QueryHash> hashes, QueryConfiguration queryConfiguration)
        {
            var allSubs = new ConcurrentBag<QueryResponseMatch>();
            int threshold = queryConfiguration.ThresholdVotes;
            var assignedClusters = queryConfiguration.Clusters;

            Parallel.ForEach(hashes, hashedFingerprint =>
            {
                var subFingerprints = ReadSubFingerprints(hashedFingerprint.Hashes, threshold, assignedClusters);
                foreach (var subFingerprint in subFingerprints)
                {
                    allSubs.Add(new QueryResponseMatch(subFingerprint, hashedFingerprint.SequenceNumber));
                }
            });

            return new FingerprintsQueryResponse(allSubs);
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
            var results = new List<uint>[hashes.Length];
            for (int table = 0; table < hashes.Length; ++table)
            {
                int hashBin = hashes[table];
                results[table] = storage.GetSubFingerprintsByHashTableAndHash(table, hashBin);
            }

            return groupingCounter.GroupByAndCount(results, thresholdVotes, id => storage.ReadSubFingerprintById(id));
        }
    }
}
