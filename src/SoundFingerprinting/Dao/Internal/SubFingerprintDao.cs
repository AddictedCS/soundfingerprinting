namespace SoundFingerprinting.Dao.Internal
{
    using SoundFingerprinting.Data;

    internal class SubFingerprintDao : AbstractDao, ISubFingerprintDao
    {
        private const string SpInsertSubFingerprint = "sp_InsertSubFingerprint";
        private const string SpReadSubFingerprintById = "sp_ReadSubFingerprintById";

        public SubFingerprintDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public SubFingerprintData ReadById(long id)
        {
            return PrepareStoredProcedure(SpReadSubFingerprintById)
                        .WithParameter("Id", id)
                        .Execute()
                        .AsComplexModel<SubFingerprintData>((item, reader) =>
                            {
                                long subFingerprintId = reader.GetInt64("Id");
                                int trackId = reader.GetInt32("TrackId");
                                item.SubFingerprintReference = new ModelReference<long>(subFingerprintId);
                                item.TrackReference = new ModelReference<int>(trackId);
                            });
        }

        public long Insert(byte[] signature, int trackId)
        {
            return PrepareStoredProcedure(SpInsertSubFingerprint)
                                .WithParameter("Signature", signature)
                                .WithParameter("TrackId", trackId)
                                .Execute()
                                .AsScalar<long>();
        }
    }
}
