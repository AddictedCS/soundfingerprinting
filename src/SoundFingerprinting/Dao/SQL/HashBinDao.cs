namespace SoundFingerprinting.Dao.SQL
{
    using System.Collections.Generic;
    using System.Text;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class HashBinDao : AbstractDao, IHashBinDao
    {
        private const string SpReadFingerprintsByHashBinHashTableAndThreshold = "sp_ReadFingerprintsByHashBinHashTableAndThreshold";

        private const string SpReadSubFingerprintsByHashBinHashTableAndThresholdWithGroupId =
            "sp_ReadSubFingerprintsByHashBinHashTableAndThresholdWithGroupId";

        private const string SpReadHashDataByTrackId = "sp_ReadHashDataByTrackId";

        public HashBinDao()
            : base(
                DependencyResolver.Current.Get<IDatabaseProviderFactory>(),
                DependencyResolver.Current.Get<IModelBinderFactory>())
        {
            // no op   
        }

        public HashBinDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
             // no op
        }

        public void Insert(long[] hashBins, long subFingerprintId)
        {
            StringBuilder sqlToExecute = new StringBuilder();
            for (int i = 0; i < hashBins.Length; i++)
            {
                sqlToExecute.Append("INSERT INTO HashTable_" + (i + 1) + "(HashBin, SubFingerprintId) VALUES(" + hashBins[i] + "," + subFingerprintId + ");");
                if (hashBins.Length > i + 1)
                {
                    sqlToExecute.Append("\n\r");
                }
            }

            PrepareSQLText(sqlToExecute.ToString()).AsNonQuery();
        }

        public IList<HashBinData> ReadHashBinsByHashTable(int hashTableId)
        {
            string sqlToExecute = "SELECT * FROM HashTable_" + hashTableId;
            return PrepareSQLText(sqlToExecute).AsListOfComplexModel<HashBinData>(
                (item, reader) =>
                    {
                        long subFingerprintId = reader.GetInt64("SubFingerprintId");
                        item.SubFingerprintReference = new ModelReference<long>(subFingerprintId);
                        item.HashTable = hashTableId;
                    });
        }

        public IList<HashData> ReadHashDataByTrackId(int trackId)
        {
            const int HashTablesCount = 25;
            return PrepareStoredProcedure(SpReadHashDataByTrackId).WithParameter("TrackId", trackId)
                .Execute()
                .AsList(reader =>
                    {
                        byte[] signature = (byte[])reader.GetRaw("Signature");
                        long[] hashBins = new long[HashTablesCount];
                        for (int i = 1; i <= HashTablesCount; i++)
                        {
                            hashBins[i - 1] = reader.GetInt64("HashBin_" + i);
                        }

                        return new HashData(signature, hashBins);
                    });
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBuckets, int thresholdVotes)
        {
            return PrepareReadSubFingerprintsByHashBuckets(SpReadFingerprintsByHashBinHashTableAndThreshold, hashBuckets, thresholdVotes)
                    .Execute()
                    .AsList(GetSubFingerprintData);
        }
        
        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] hashBuckets, int thresholdVotes, string trackGroupId)
        {
            return PrepareReadSubFingerprintsByHashBuckets(
                    SpReadSubFingerprintsByHashBinHashTableAndThresholdWithGroupId, hashBuckets, thresholdVotes)
                           .WithParameter("GroupId", trackGroupId)
                           .Execute()
                           .AsList(GetSubFingerprintData);
        }

        private IParameterBinder PrepareReadSubFingerprintsByHashBuckets(string storedProcedure, long[] hashBuckets, int thresholdVotes)
        {
            var parameterBinder = PrepareStoredProcedure(storedProcedure);
            for (int i = 1; i <= hashBuckets.Length; i++)
            {
                parameterBinder = parameterBinder.WithParameter("HashBin_" + i, hashBuckets[i - 1]);
            }

            return parameterBinder.WithParameter("Threshold", thresholdVotes);
        }

        private SubFingerprintData GetSubFingerprintData(IReader reader)
        {
            long id = reader.GetInt64("Id");
            byte[] signature = (byte[])reader.GetRaw("Signature");
            int trackId = reader.GetInt32("TrackId");
            return new SubFingerprintData(signature, new ModelReference<long>(id), new ModelReference<int>(trackId));
        }
    }
}
