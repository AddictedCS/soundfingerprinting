namespace SoundFingerprinting.InMemory
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
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

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference)
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

                storage.TracksHashes[trackReference][subFingerprintReference].HashBins = hashBins;
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

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(
            long[] hashBins, int thresholdVotes)
        {
            int table = 0;
            var hashTables = storage.HashTables;
            var subFingeprintCount = new Dictionary<IModelReference, int>();
            foreach (var hashBin in hashBins)
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

        public ISet<SubFingerprintData> ReadAllSubFingerprintCandidatesWithThreshold(IEnumerable<HashedFingerprint> hashes, int threshold)
        {
            var allCandidates = new HashSet<SubFingerprintData>();
            foreach (var hashedFingerprint in hashes)
            {
                var subFingerprints = ReadSubFingerprintDataByHashBucketsWithThreshold(hashedFingerprint.HashBins, threshold);
                foreach (var subFingerprint in subFingerprints)
                {
                    allCandidates.Add(subFingerprint);
                }
            }

            return allCandidates;
        }
    }
}
