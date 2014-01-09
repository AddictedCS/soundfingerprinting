namespace SoundFingerprinting.Dao.Internal
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Dao.Entities;

    internal class HashBinMinHashDao : AbstractDao
    {
        private const string SpInsertMinhashHashbin = "sp_InsertHashBinMinHash";
        private const string SpReadFingerprintsByHashBinHashTableAndThreshold = "sp_ReadFingerprintsByHashBinHashTableAndThreshold";
        private const string SpReadHashBinsByHashTable = "sp_ReadHashBinsByHashTable";

        public HashBinMinHashDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
             // no op
        }

        public void Insert(HashBinMinHash hashBin)
        {
            PrepareStoredProcedure(SpInsertMinhashHashbin)
                            .WithParametersFromModel(hashBin)
                            .Execute()
                            .AsNonQuery();
        }

        public IList<HashBinMinHash> ReadHashBinsByHashTable(int hashTable)
        {
            return PrepareStoredProcedure(SpReadHashBinsByHashTable)
                .WithParameter("HashTable", hashTable)
                .Execute()
                .AsList(reader => new HashBinMinHash(reader.GetInt64("HashBin"), hashTable, reader.GetInt64("SubFingerprintId")));
        }

        public IEnumerable<Tuple<SubFingerprint, int>> ReadSubFingerprintsByHashBucketsHavingThreshold(long[] hashBuckets, int thresholdVotes)
        {
            return PrepareStoredProcedure(SpReadFingerprintsByHashBinHashTableAndThreshold)
                    .WithParameter("HashBin_1", hashBuckets[0])
                    .WithParameter("HashBin_2", hashBuckets[1])
                    .WithParameter("HashBin_3", hashBuckets[2])
                    .WithParameter("HashBin_4", hashBuckets[3])
                    .WithParameter("HashBin_5", hashBuckets[4])
                    .WithParameter("HashBin_6", hashBuckets[5])
                    .WithParameter("HashBin_7", hashBuckets[6])
                    .WithParameter("HashBin_8", hashBuckets[7])
                    .WithParameter("HashBin_9", hashBuckets[8])
                    .WithParameter("HashBin_10", hashBuckets[9])
                    .WithParameter("HashBin_11", hashBuckets[10])
                    .WithParameter("HashBin_12", hashBuckets[11])
                    .WithParameter("HashBin_13", hashBuckets[12])
                    .WithParameter("HashBin_14", hashBuckets[13])
                    .WithParameter("HashBin_15", hashBuckets[14])
                    .WithParameter("HashBin_16", hashBuckets[15])
                    .WithParameter("HashBin_17", hashBuckets[16])
                    .WithParameter("HashBin_18", hashBuckets[17])
                    .WithParameter("HashBin_19", hashBuckets[18])
                    .WithParameter("HashBin_20", hashBuckets[19])
                    .WithParameter("HashBin_21", hashBuckets[20])
                    .WithParameter("HashBin_22", hashBuckets[21])
                    .WithParameter("HashBin_23", hashBuckets[22])
                    .WithParameter("HashBin_24", hashBuckets[23])
                    .WithParameter("HashBin_25", hashBuckets[24])
                    .WithParameter("Threshold", thresholdVotes)
                    .Execute()
                    .AsList(reader =>
                        {
                            long id = reader.GetInt64("Id");
                            byte[] signature = (byte[])reader.GetRaw("Signature");
                            int trackId = reader.GetInt32("TrackId");
                            int votes = reader.GetInt32("Votes");
                            return new Tuple<SubFingerprint, int>(new SubFingerprint(signature, trackId) { Id = id }, votes);
                        });
        }
    }
}
