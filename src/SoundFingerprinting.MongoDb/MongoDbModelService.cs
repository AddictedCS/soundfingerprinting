namespace SoundFingerprinting.MongoDb
{
    using SoundFingerprinting.DAO;

    public class MongoDbModelService : ModelService
    {
        public MongoDbModelService()
            : this(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao(), new SpectralImageDao())
        {
        }

        protected MongoDbModelService(ITrackDao trackDao, IHashBinDao hashBinDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao, ISpectralImageDao spectralImageDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao, spectralImageDao)
        {
        }
    }
}
