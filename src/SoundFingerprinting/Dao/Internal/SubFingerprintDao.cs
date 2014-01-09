namespace SoundFingerprinting.Dao.Internal
{
    using System.Collections.Generic;

    using SoundFingerprinting.Dao.Entities;

    internal class SubFingerprintDao : AbstractDao
    {
        private const string SpInsertSubFingerprint = "sp_InsertSubFingerprint";
        private const string SpReadSubFingerprintById = "sp_ReadSubFingerprintById";

        public SubFingerprintDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public SubFingerprint ReadById(long id)
        {
            return PrepareStoredProcedure(SpReadSubFingerprintById)
                        .Execute()
                        .AsModel<SubFingerprint>();
        }

        public void Insert(SubFingerprint subFingerprint)
        {
            subFingerprint.Id = PrepareStoredProcedure(SpInsertSubFingerprint)
                                .WithParametersFromModel(subFingerprint)
                                .Execute()
                                .AsScalar<long>();
        }

        public void Insert(IEnumerable<SubFingerprint> collection)
        {
            foreach (var subFingerprint in collection)
            {
                Insert(subFingerprint);
            }
        }
    }
}
