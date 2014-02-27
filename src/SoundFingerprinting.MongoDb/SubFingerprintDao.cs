namespace SoundFingerprinting.MongoDb
{
    using System;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;

    internal class SubFingerprintDao : AbstractDao, ISubFingerprintDao
    {
        public SubFingerprintDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
            // no op
        }

        public SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference)
        {
            throw new NotImplementedException();
        }

        public IModelReference InsertSubFingerprint(byte[] signature, IModelReference trackReference)
        {
            throw new NotImplementedException();
        }
    }
}
