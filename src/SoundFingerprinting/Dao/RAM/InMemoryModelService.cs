namespace SoundFingerprinting.Dao.RAM
{
    public class InMemoryModelService : ModelService
    {
        public InMemoryModelService()
            : base(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao())
        {
            // no op
        }

        protected InMemoryModelService(ITrackDao trackDao, IHashBinDao hashBinDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao)
        {
            // no op
        }
    }
}
