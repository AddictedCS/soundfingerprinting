namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly IRAMStorage storage;
        private readonly IHashConverter hashConverter;

        public SubFingerprintDao(): this(DependencyResolver.Current.Get<IRAMStorage>(), DependencyResolver.Current.Get<IHashConverter>())
        {
        }

        public SubFingerprintDao(IRAMStorage storage, IHashConverter hashConverter)
        {
            this.storage = storage;
            this.hashConverter = hashConverter;
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

            return subFingerprints.Select(
                data =>
                {
                    var byteArray = hashConverter.ToBytes(data.Hashes, 100);
                    return new HashedFingerprint(
                        byteArray,
                        data.Hashes,
                        data.SequenceNumber,
                        data.SequenceAt,
                        data.Clusters);
                }).ToList();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashes, int thresholdVotes, IEnumerable<string> assignedClusters)
        {
            var subFingeprintCount = CountSubFingerprintMatches(hashes);
            var subFingerprints = subFingeprintCount.Where(pair => pair.Value >= thresholdVotes)
                                                    .Select(pair => storage.ReadSubFingerprintById(pair.Key));

            var clusters = assignedClusters as List<string> ?? assignedClusters.ToList();
            if (clusters.Any())
            {
                return subFingerprints.Where(subFingerprint => subFingerprint.Clusters.Intersect(clusters).Any());
            }

            return subFingerprints;
        }

        public ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<long[]> hashes, int threshold, IEnumerable<string> assignedClusters)
        {
            var allCandidates = new ConcurrentDictionary<SubFingerprintData, byte>();
            Parallel.ForEach(
                hashes,
                hashedFingerprint =>
                    {
                        var subFingerprints = ReadSubFingerprints(hashedFingerprint, threshold, assignedClusters);
                        foreach (var subFingerprint in subFingerprints)
                        {
                            allCandidates.AddOrUpdate(subFingerprint, 0, (data, b) => 0);
                        }
                    });

            return new HashSet<SubFingerprintData>(allCandidates.Keys.ToList());
        }

        private Dictionary<ulong, int> CountSubFingerprintMatches(long[] hashes)
        {
            var subFingeprintCount = new Dictionary<ulong, int>();
            for (int table = 0; table < hashes.Length; ++table)
            {
                var hashBin = hashes[table];
                foreach (var subFingerprintId in storage.GetSubFingerprintsByHashTableAndHash(table, hashBin))
                {
                    IncrementSubFingerprintCount(subFingeprintCount, subFingerprintId);
                }
            }

            return subFingeprintCount;
        }

        private void IncrementSubFingerprintCount(IDictionary<ulong, int> subFingeprintCount, ulong subFingerprintId)
        {
            if (!subFingeprintCount.ContainsKey(subFingerprintId))
            {
                subFingeprintCount[subFingerprintId] = 0;
            }

            subFingeprintCount[subFingerprintId]++;
        }
    }
}
