namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;

    using MongoDB.Bson;
    using MongoDB.Driver.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.Data;
    using SoundFingerprinting.MongoDb.Entity;

    internal class HashBinDao : AbstractDao, IHashBinDao
    {
        private const string HashBins = "HashBins";

        public HashBinDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
        }

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference)
        {
            var collection = GetCollection<Hash>(HashBins);
            collection.Insert(
                new Hash
                    {
                        HashBins = hashBins,
                        SubFingerprintId = (ObjectId)subFingerprintReference.Id,
                        TrackId = (ObjectId)trackReference.Id
                    });
        }

        public IList<HashData> ReadHashDataByTrackReference(IModelReference trackReference)
        {
            var hashes = GetCollection<Hash>(HashBins)
                                .AsQueryable()
                                .Where(h => h.TrackId.Equals(trackReference.Id));
            var hashDatas = new List<HashData>();
            foreach(var hash in hashes)
            {
                var subFingerprint = GetCollection<SubFingerprint>(SubFingerprintDao.SubFingerprints)
                                                        .AsQueryable()
                                                        .First(s => s.Id.Equals(hash.SubFingerprintId));
                var hashData = new HashData(subFingerprint.Signature, hash.HashBins);
                hashDatas.Add(hashData);
            }

            return hashDatas;
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBins, int thresholdVotes)
        {
            var filteredSubFingerprints =
                GetCollection<Hash>(HashBins).AsQueryable().Where(hash => FilterHashes(hashBins, thresholdVotes, hash)).
                    Select(h => h.SubFingerprintId);

            var subs = new List<SubFingerprintData>();
            foreach (var id in filteredSubFingerprints)
            {
                var subFingerprint =
                    GetCollection<SubFingerprint>(SubFingerprintDao.SubFingerprints).AsQueryable().First(
                        s => s.Id.Equals(id));
                var sub = new SubFingerprintData(
                    subFingerprint.Signature,
                    new MongoModelReference(subFingerprint.Id),
                    new MongoModelReference(subFingerprint.TrackId));
                subs.Add(sub);
            }

            return subs;
        }

        private static bool FilterHashes(long[] hashBins, int thresholdVotes, Hash hash)
        {
            int tableCount = 0;
            for (int i = 0; i < hashBins.Length; i++)
            {
                if (hash.HashBins[i] == hashBins[i])
                {
                    tableCount++;

                    if (tableCount >= thresholdVotes)
                    {
                        break;
                    }
                }
            }

            return tableCount >= thresholdVotes;
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] hashBuckets, int thresholdVotes, string trackGroupId)
        {
            var subFingerprints = ReadSubFingerprintDataByHashBucketsWithThreshold(hashBuckets, thresholdVotes);

            var tracksWithGroupId = GetCollection<Track>(TrackDao.Tracks).AsQueryable()
                                                                         .Where(t => t.GroupId.Equals(trackGroupId))
                                                                         .Select(t => t.Id);
            return subFingerprints.Where(s => tracksWithGroupId.Contains((ObjectId)s.TrackReference.Id));
        }
    }
}
