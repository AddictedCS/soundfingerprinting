namespace SoundFingerprinting.MongoDb
{
    using SoundFingerprinting.DAO;

    public class MongoDbModelService : ModelService
    {
        public MongoDbModelService()
            : this(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao())
        {
        }

        protected MongoDbModelService(ITrackDao trackDao, IHashBinDao hashBinDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao)
        {
        }
    }
}
