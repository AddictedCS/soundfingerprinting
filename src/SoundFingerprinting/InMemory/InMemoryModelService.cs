namespace SoundFingerprinting.InMemory
{
    using SoundFingerprinting.DAO;

    public class InMemoryModelService : ModelService
    {
        public InMemoryModelService()
            : base(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao())
        {
            // no op
        }

        internal InMemoryModelService(ITrackDao trackDao, IHashBinDao hashBinDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao)
        {
            // no op
        }
    }
}
