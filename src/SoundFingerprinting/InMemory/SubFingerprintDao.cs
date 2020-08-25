namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
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

        public int SubFingerprintsCount => storage.SubFingerprintsCount;

        public IEnumerable<int> HashCountsPerTable => storage.HashCountsPerTable;

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
            var allSubs = new ConcurrentDictionary<uint, byte>();
            int threshold = queryConfiguration.ThresholdVotes;

            Parallel.ForEach(hashes, hashedFingerprint =>
            {
                var ids = QuerySubFingerprints(hashedFingerprint, threshold);
                foreach (uint subFingerprint in ids)
                {
                    allSubs.TryAdd(subFingerprint, 0);
                }
            });

            return ResolveFromIds(allSubs.Keys, queryConfiguration.MetaFieldsFilter);
        }

        public int DeleteSubFingerprintsByTrackReference(IModelReference trackReference)
        {
            return storage.DeleteSubFingerprintsByTrackReference(trackReference);
        }

        private IEnumerable<uint> QuerySubFingerprints(int[] hashes, int thresholdVotes)
        {
            var results = new List<uint>[hashes.Length];
            for (int table = 0; table < hashes.Length; ++table)
            {
                int hashBin = hashes[table];
                results[table] = storage.GetSubFingerprintsByHashTableAndHash(table, hashBin);
            }

            return groupingCounter.GroupByAndCount(results, thresholdVotes);
        }
        
        private IEnumerable<SubFingerprintData> ResolveFromIds(IEnumerable<uint> ids, IDictionary<string, string> metaFieldsFilter)
        {
            if (metaFieldsFilter.Any())
            {
                return ids.Select(storage.ReadSubFingerprintById)
                    .GroupBy(subFingerprint => subFingerprint.TrackReference)
                    .Where(group =>
                    {
                        if(storage.Tracks.TryGetValue(group.Key, out var trackData))
                        {
                            return trackData.MetaFields
                                    .Join(metaFieldsFilter, _ => _.Key, _ => _.Key, (a, b) => a.Value.Equals(b.Value))
                                    .Any(x => x);
                        }

                        return false;
                    })
                    .SelectMany(x =>x.ToList());
            }

            return ids.Select(storage.ReadSubFingerprintById);
        }
    }
}
