namespace Soundfingerprinting.Dao.Internal
{
    using System;
    using System.Collections.Generic;

    using Soundfingerprinting.Dao.Entities;

    internal class HashBinDao : AbstractDao
    {
        private const string SpInsertMinhashHashbin = "sp_InsertHashBinMinHash";
        private const string SpReadHashbinsByHashBin = "sp_ReadHashBinsByHashBinsMinHash";

        public HashBinDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public void Insert(HashBinMinHash hashBin)
        {
            hashBin.Id = PrepareStoredProcedure(SpInsertMinhashHashbin)
                            .WithParametersFromModel(hashBin)
                            .Execute()
                            .AsScalar<int>();
        }

        public IDictionary<int, IList<HashBinMinHash>> ReadFingerprintsByHashBucketLSH(long[] hashBuckets)
        {
            IDictionary<int, IList<HashBinMinHash>> result = new Dictionary<int, IList<HashBinMinHash>>();
            foreach (var hashBucket in hashBuckets)
            {
                long hashBin = hashBucket;
                var resultPerHashBucket =
                    PrepareStoredProcedure(SpReadHashbinsByHashBin).WithParameter("Bin", hashBucket).Execute().
                        AsDictionary(
                            (reader) => reader.GetInt32("FingerprintId"),
                            (reader) =>
                                {
                                    int hashId = reader.GetInt32("Id");
                                    int trackId = reader.GetInt32("TrackId");
                                    int fingerprintId = reader.GetInt32("FingerprintId");
                                    int hashtable = reader.GetInt32("HashTable");
                                    return new HashBinMinHash(hashId, hashBin, hashtable, trackId, fingerprintId);
                                });

                foreach (var pair in resultPerHashBucket)
                {
                    if (result.ContainsKey(pair.Key))
                    {
                        result[pair.Key].Add(pair.Value);
                    }
                    else
                    {
                        result.Add(pair.Key, new List<HashBinMinHash>(new[] { pair.Value }));
                    }
                }
            }

            return result;
        }
    }
}