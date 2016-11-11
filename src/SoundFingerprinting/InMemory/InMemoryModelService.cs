namespace SoundFingerprinting.InMemory
{
    using SoundFingerprinting.DAO;

    public class InMemoryModelService : AdvancedModelService
    {
        public InMemoryModelService()
            : this(new TrackDao(), new SubFingerprintDao(), new FingerprintDao(), new SpectralImageDao())
        {
            // no op
        }

        internal InMemoryModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao, ISpectralImageDao spectralImageDao)
            : base(trackDao, subFingerprintDao, fingerprintDao, spectralImageDao)
        {
            // no op
        }
    }
}
