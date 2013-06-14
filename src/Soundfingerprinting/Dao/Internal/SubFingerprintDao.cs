namespace SoundFingerprinting.Dao.Internal
{
    using System.Collections.Generic;

    using SoundFingerprinting.Dao.Entities;

    internal class SubFingerprintDao : AbstractDao
    {
        private const string SpInsertSubFingerprint = "sp_InsertSubFingerprint";

        public SubFingerprintDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
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
