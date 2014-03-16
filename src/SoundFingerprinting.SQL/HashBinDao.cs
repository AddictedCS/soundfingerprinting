namespace SoundFingerprinting.SQL
{
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class HashBinDao : AbstractDao, IHashBinDao
    {
        private const string SpReadFingerprintsByHashBinHashTableAndThreshold = "sp_ReadFingerprintsByHashBinHashTableAndThreshold";

        private const string SpReadSubFingerprintsByHashBinHashTableAndThresholdWithGroupId =
            "sp_ReadSubFingerprintsByHashBinHashTableAndThresholdWithGroupId";

        private const string SpReadHashDataByTrackId = "sp_ReadHashDataByTrackId";

        private const int HashTablesCount = 25;

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

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference)
        {
            StringBuilder sqlToExecute = new StringBuilder();
            for (int hashTable = 0; hashTable < hashBins.Length; hashTable++)
            {
                sqlToExecute.Append("INSERT INTO HashTable_" + (hashTable + 1) + "(HashBin, SubFingerprintId) VALUES(" + hashBins[hashTable] + "," + subFingerprintReference.Id + ");");
                if (hashBins.Length > hashTable + 1)
                {
                    sqlToExecute.Append("\n\r");
                }
            }

            PrepareSQLText(sqlToExecute.ToString()).AsNonQuery();
        }

        public IList<HashData> ReadHashDataByTrackReference(IModelReference trackReference)
        {
            return PrepareStoredProcedure(SpReadHashDataByTrackId)
                .WithParameter("TrackId", trackReference.Id, DbType.Int32)
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

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBins, int thresholdVotes)
        {
            return PrepareReadSubFingerprintsByHashBuckets(SpReadFingerprintsByHashBinHashTableAndThreshold, hashBins, thresholdVotes)
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
            for (int hashTable = 1; hashTable <= hashBuckets.Length; hashTable++)
            {
                parameterBinder = parameterBinder.WithParameter("HashBin_" + hashTable, hashBuckets[hashTable - 1]);
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
