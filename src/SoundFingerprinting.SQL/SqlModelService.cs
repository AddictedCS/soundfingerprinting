namespace SoundFingerprinting.SQL
{
    using SoundFingerprinting.DAO;

    public class SqlModelService : ModelService
    {
        public SqlModelService()
            : base(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao(), new SpectralImageDao())
        {
            // no op
        }

        protected SqlModelService(
            ITrackDao trackDao,
            IHashBinDao hashBinDao,
            ISubFingerprintDao subFingerprintDao,
            IFingerprintDao fingerprintDao,
            ISpectralImageDao spectralImageDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao, spectralImageDao)
        {
            // no op
        }
    }
}
