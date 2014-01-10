namespace SoundFingerprinting.Dao.Internal
{
    using System.Collections.Generic;
    using System.Text;

    using SoundFingerprinting.Data;

    internal class HashBinDao : AbstractDao
    {
        private const string SpReadFingerprintsByHashBinHashTableAndThreshold = "sp_ReadFingerprintsByHashBinHashTableAndThreshold";
        
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

        public IList<HashBinData> ReadHashBinsByHashTable(int hashTable)
        {
            string sqlToExecute = "SELECT * FROM HashTable_" + hashTable;
            return PrepareSQLText(sqlToExecute).AsListOfComplexModel<HashBinData>(
                (item, reader) =>
                    {
                        long subFingerprintId = reader.GetInt64("SubFingerprintId");
                        item.SubFingerprintReference = new RDBMSSubFingerprintReference(subFingerprintId);
                        item.HashTable = hashTable;
                    });
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBuckets, int thresholdVotes)
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
                            return new SubFingerprintData(
                                signature, new RDBMSSubFingerprintReference(id), new RDBMSTrackReference(trackId));
                        });
        }
    }
}
