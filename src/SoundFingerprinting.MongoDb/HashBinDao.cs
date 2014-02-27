namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;

    internal class HashBinDao : AbstractDao, IHashBinDao
    {
        public HashBinDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
            // no op
        }

        public void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference)
        {
            throw new System.NotImplementedException();
        }

        public IList<HashData> ReadHashDataByTrackReference(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBuckets, int thresholdVotes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] hashBuckets, int thresholdVotes, string trackGroupId)
        {
            throw new System.NotImplementedException();
        }
    }
}
