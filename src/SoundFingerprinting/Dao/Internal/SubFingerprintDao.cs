namespace SoundFingerprinting.Dao.Internal
{
    using SoundFingerprinting.Data;

    internal class SubFingerprintDao : AbstractDao
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
                        .Execute()
                        .AsModel<SubFingerprintData>();
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
