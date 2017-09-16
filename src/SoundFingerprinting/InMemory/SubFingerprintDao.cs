namespace SoundFingerprinting.InMemory
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private static long counter;

        private readonly IRAMStorage storage;

        public SubFingerprintDao() : this(DependencyResolver.Current.Get<IRAMStorage>())
        {
        }

        public SubFingerprintDao(IRAMStorage storage)
        {
            this.storage = storage;
        }

        public SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference)
        {
            if (storage.SubFingerprints.ContainsKey(subFingerprintReference))
            {
                return storage.SubFingerprints[subFingerprintReference];
            }

            return null;
        }
        
        public void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            foreach (var hashedFingerprint in hashes)
            {
                InsertSubFingerprint(hashedFingerprint, trackReference);
            }
        }

        public IList<HashedFingerprint> ReadHashedFingerprintsByTrackReference(IModelReference trackReference)
        {
            if (storage.TracksHashes.ContainsKey(trackReference))
            {
                return storage.TracksHashes[trackReference].Values.ToList();
            }

            return Enumerable.Empty<HashedFingerprint>().ToList();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprints(long[] hashes, int thresholdVotes, IEnumerable<string> assignedClusters)
        {
            var subFingeprintCount = CountSubFingerprintMatches(hashes);
            var subFingerprints = subFingeprintCount.Where(pair => pair.Value >= thresholdVotes)
                                                    .Select(pair => storage.SubFingerprints[pair.Key]);

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

        private Dictionary<IModelReference, int> CountSubFingerprintMatches(long[] hashes)
        {
            var hashTables = this.storage.HashTables;
            var subFingeprintCount = new Dictionary<IModelReference, int>();

            for (int table = 0; table < hashes.Length; ++table)
            {
                var hashBin = hashes[table];
                if (hashTables[table].ContainsKey(hashBin))
                {
                    foreach (var subFingerprintId in hashTables[table][hashBin])
                    {
                        IncrementSubFingerprintCount(subFingeprintCount, subFingerprintId);
                    }
                }
            }

            return subFingeprintCount;
        }

        private void IncrementSubFingerprintCount(IDictionary<IModelReference, int> subFingeprintCount, IModelReference subFingerprintId)
        {
            if (!subFingeprintCount.ContainsKey(subFingerprintId))
            {
                subFingeprintCount[subFingerprintId] = 0;
            }

            subFingeprintCount[subFingerprintId]++;
        }

        private void InsertHashes(long[] hashBins, IModelReference subFingerprintReference)
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
            }
        }

        private void InsertSubFingerprint(HashedFingerprint hashedFingerprint, IModelReference trackReference)
        {
            var subFingerprintReference = new ModelReference<long>(Interlocked.Increment(ref counter));
            storage.SubFingerprints[subFingerprintReference] = new SubFingerprintData(
                hashedFingerprint.HashBins,
                hashedFingerprint.SequenceNumber,
                hashedFingerprint.StartsAt,
                subFingerprintReference,
                trackReference) {
                                    Clusters = hashedFingerprint.Clusters 
                                };
            if (!storage.TracksHashes.ContainsKey(trackReference))
            {
                storage.TracksHashes[trackReference] = new ConcurrentDictionary<IModelReference, HashedFingerprint>();
            }

            storage.TracksHashes[trackReference][subFingerprintReference] = hashedFingerprint;
            this.InsertHashes(hashedFingerprint.HashBins, subFingerprintReference);
        }
    }
}
