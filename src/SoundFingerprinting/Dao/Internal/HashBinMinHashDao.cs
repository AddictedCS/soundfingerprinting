namespace SoundFingerprinting.Dao.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using SoundFingerprinting.Dao.Entities;

    internal class HashBinMinHashDao : AbstractDao
    {
        private const string SpInsertMinhashHashbin = "sp_InsertHashBinMinHash";
        private const string SpReadAllHashBinsFromHashTableMinHash = "sp_ReadAllHashBinsFromHashTableMinHash";
        private const string SpReadFingerprintsByHashBinHashTableAndThreshold = "sp_ReadFingerprintsByHashBinHashTableAndThreshold";

        public HashBinMinHashDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
             // no op
        }

        public void Insert(HashBinMinHash hashBin)
        {
            hashBin.Id = PrepareStoredProcedure(SpInsertMinhashHashbin)
                            .WithParametersFromModel(hashBin)
                            .Execute()
                            .AsScalar<int>();
        }

        public IEnumerable<HashBinMinHash> ReadAll()
        {
            return PrepareStoredProcedure(SpReadAllHashBinsFromHashTableMinHash).Execute().AsListOfModel<HashBinMinHash>();
        }

        public IEnumerable<Tuple<SubFingerprint, int>> ReadSubFingerprintsByHashBucketsHavingThreshold(long[] hashBuckets, int thresholdVotes)
        {
            const string Delimiter = ";";
            StringBuilder builder = new StringBuilder();
            foreach (long hashBucket in hashBuckets)
            {
                builder.Append(hashBucket + Delimiter);
            }

           return PrepareStoredProcedure(SpReadFingerprintsByHashBinHashTableAndThreshold)
                    .WithParameter("ConcatHashBucket", builder.ToString())
                    .WithParameter("Delimiter", Delimiter)
                    .WithParameter("Threshold", thresholdVotes)
                    .Execute()
                    .AsList(reader =>
                        {
                            long id = reader.GetInt64("Id");
                            byte[] signature = (byte[])reader.GetRaw("Signature");
                            int trackId = reader.GetInt32("TrackId");
                            int votes = reader.GetInt32("Votes");
                            return new Tuple<SubFingerprint, int>(
                                new SubFingerprint(signature, trackId)
                                    { Id = id },
                                votes);
                        });
        }
    }
}