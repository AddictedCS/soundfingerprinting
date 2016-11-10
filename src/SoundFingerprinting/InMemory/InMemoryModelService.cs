namespace SoundFingerprinting.InMemory
{
    using SoundFingerprinting.DAO;

    public class InMemoryModelService : AdvancedModelService
    {
        public InMemoryModelService()
            : this(new TrackDao(), new HashBinDao(), new SubFingerprintDao(), new FingerprintDao(), new SpectralImageDao())
        {
            // no op
        }

        internal InMemoryModelService(ITrackDao trackDao, IHashBinDao hashBinDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao, ISpectralImageDao spectralImageDao)
            : base(trackDao, hashBinDao, subFingerprintDao, fingerprintDao, spectralImageDao)
        {
            // no op
        }
    }
}
