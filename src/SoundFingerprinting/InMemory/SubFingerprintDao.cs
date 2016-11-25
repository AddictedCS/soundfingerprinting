namespace SoundFingerprinting.InMemory
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

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
            int table = 0;
            var hashTables = storage.HashTables;
            var subFingeprintCount = new Dictionary<IModelReference, int>();
            foreach (var hashBin in hashes)
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

            var subFingerprints = subFingeprintCount.Where(pair => pair.Value >= thresholdVotes)
                                                    .Select(pair => storage.SubFingerprints[pair.Key]);
            var clusters = assignedClusters as List<string> ?? assignedClusters.ToList();
            if (clusters.Any())
            {
                return subFingerprints.Where(subFingerprint => subFingerprint.AssignedClusters.Intersect(clusters).Any());
            }

            return subFingerprints;
        }

        public ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<long[]> hashes, int threshold, IEnumerable<string> assignedClusters)
        {
            var allCandidates = new HashSet<SubFingerprintData>();
            foreach (var hashedFingerprint in hashes)
            {
                var subFingerprints = ReadSubFingerprints(hashedFingerprint, threshold, assignedClusters);
                foreach (var subFingerprint in subFingerprints)
                {
                    allCandidates.Add(subFingerprint);
                }
            }

            return allCandidates;
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
                                    AssignedClusters = hashedFingerprint.AssignedClusters 
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
