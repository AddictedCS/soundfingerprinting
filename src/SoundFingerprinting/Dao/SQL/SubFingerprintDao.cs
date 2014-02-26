namespace SoundFingerprinting.Dao.SQL
{
    using System.Data;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class SubFingerprintDao : AbstractDao, ISubFingerprintDao
    {
        private const string SpInsertSubFingerprint = "sp_InsertSubFingerprint";
        private const string SpReadSubFingerprintById = "sp_ReadSubFingerprintById";

        public SubFingerprintDao() : base(
                DependencyResolver.Current.Get<IDatabaseProviderFactory>(),
                DependencyResolver.Current.Get<IModelBinderFactory>())
        {
            // no op
        }

        public SubFingerprintDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference)
        {
            return PrepareStoredProcedure(SpReadSubFingerprintById)
                        .WithParameter("Id", subFingerprintReference.Id, DbType.Int64)
                        .Execute()
                        .AsComplexModel<SubFingerprintData>((item, reader) =>
                            {
                                item.SubFingerprintReference = new ModelReference<long>(reader.GetInt64("Id"));
                                item.TrackReference = new ModelReference<int>(reader.GetInt32("TrackId"));
                            });
        }

        public IModelReference InsertSubFingerprint(byte[] signature, IModelReference trackReference)
        {
            long subFingerprintId = PrepareStoredProcedure(SpInsertSubFingerprint)
                                .WithParameter("Signature", signature)
                                .WithParameter("TrackId", trackReference.Id, DbType.Int32)
                                .Execute()
                                .AsScalar<long>();

            return new ModelReference<long>(subFingerprintId);
        }
    }
}
