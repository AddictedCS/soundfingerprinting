namespace SoundFingerprinting.SQL
{
    using SoundFingerprinting.DAO;

    public class SqlModelService : ModelService
    {
        public SqlModelService()
            : base(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao())
        {
            // no op
        }

        protected SqlModelService(
            ITrackDao trackDao,
            IHashBinDao hashBinDao,
            ISubFingerprintDao subFingerprintDao,
            IFingerprintDao fingerprintDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao)
        {
            // no op
        }
    }
}
