namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.DAO;
    using SoundFingerprinting.MongoDb.Entity;

    internal class HashBinDao : AbstractDao, IHashBinDao
    {
        public const string HashBins = "HashBins";

        public HashBinDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
        }

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference)
        {
            var hashes = new List<Hash>();
            for (int hashtable = 1; hashtable <= hashBins.Length; hashtable++)
            {
                var hash = new Hash
                    {
                        HashTable = hashtable,
                        HashBin = hashBins[hashtable - 1],
                        SubFingerprintId = (ObjectId)subFingerprintReference.Id,
                        TrackId = (ObjectId)trackReference.Id
                    };
                hashes.Add(hash);
            }

            GetCollection<Hash>(HashBins).InsertBatch(hashes);
        }

        public IList<HashData> ReadHashDataByTrackReference(IModelReference trackReference)
        {
            var hashes = GetCollection<Hash>(HashBins)
                                .AsQueryable()
                                .Where(hash => hash.TrackId.Equals(trackReference.Id))
                                .ToList();

            var subFingerprintIds = hashes.GroupBy(hash => hash.SubFingerprintId).Select(g => g.Key);

            var hashDatas = new List<HashData>();
            foreach (var subfingerprintId in subFingerprintIds)
            {
                var hashBins = hashes.Where(hash => hash.SubFingerprintId.Equals(subfingerprintId))
                                     .OrderBy(hash => hash.HashTable)
                                     .Select(hash => hash.HashBin)
                                     .ToArray();
                var subFingerprint = GetCollection<SubFingerprint>(SubFingerprintDao.SubFingerprints)
                                                        .AsQueryable()
                                                        .First(s => s.Id.Equals(subfingerprintId));
                var hashData = new HashData(subFingerprint.Signature, hashBins);
                hashDatas.Add(hashData);
            }

            return hashDatas;
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBins, int thresholdVotes)
        {
            var queries = new List<IMongoQuery>();
            for (int hashtable = 1; hashtable <= hashBins.Length; hashtable++)
            {
                var and = MongoDB.Driver.Builders.Query.And(
                    MongoDB.Driver.Builders.Query.EQ("HashTable", hashtable),
                    MongoDB.Driver.Builders.Query.EQ("HashBin", hashBins[hashtable - 1]));
                queries.Add(and);
            }

            var query = BsonValue.Create(MongoDB.Driver.Builders.Query.Or(queries));
            var match = new BsonDocument { { "$match", query } };
            var group = new BsonDocument
                {
                    {
                        "$group",
                        new BsonDocument
                            {
                                {
                                    "_id",
                                    new BsonDocument
                                        {
                                            { "SubFingerprintId", "$SubFingerprintId" }
                                        }
                                },
                                { "Votes", new BsonDocument { { "$sum", 1 } } }
                            }
                    }
                };

            var thresholdMatch = new BsonDocument
                {
                    { "$match", new BsonDocument { { "Votes", new BsonDocument { { "$gte", thresholdVotes } } } } } 
                };

            var project = new BsonDocument
                {
                    {
                        "$project",
                        new BsonDocument
                            {
                                { "_id", 0 },
                                { "SubFingerprintId", "$_id.SubFingerprintId" }
                            }
                    }
                };

            var result = GetCollection<Hash>(HashBins).Aggregate(new[] { match, group, thresholdMatch, project });

            if (!result.ResultDocuments.Any())
            {
                return Enumerable.Empty<SubFingerprintData>().ToList();
            }

            var subFingerprintDatas = new List<SubFingerprintData>();
            foreach (var resultDocument in result.ResultDocuments)
            {
                var objectId = (ObjectId)resultDocument.GetValue("SubFingerprintId");
                var subFingerprint = GetCollection<SubFingerprint>(SubFingerprintDao.SubFingerprints)
                                         .AsQueryable()
                                         .First(s => s.Id.Equals(objectId));

                subFingerprintDatas.Add(new SubFingerprintData
                    {
                        Signature = subFingerprint.Signature,
                        SubFingerprintReference = new MongoModelReference(objectId),
                        TrackReference = new MongoModelReference(subFingerprint.TrackId)
                    });
            }

            return subFingerprintDatas;
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] hashBuckets, int thresholdVotes, string trackGroupId)
        {
            var tracksWithGroupId = GetCollection<Track>(TrackDao.Tracks).AsQueryable()
                                                                     .Where(track => track.GroupId.Equals(trackGroupId))
                                                                     .Select(track => track.Id)
                                                                     .ToList();
            
            return ReadSubFingerprintDataByHashBucketsWithThreshold(hashBuckets, thresholdVotes).Where(s => tracksWithGroupId.Contains((ObjectId)s.TrackReference.Id));
        }
    }
}
