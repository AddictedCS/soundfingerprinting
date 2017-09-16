namespace SoundFingerprinting.InMemory
{
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Infrastructure;

    public class InMemoryModelService : AdvancedModelService
    {
        private readonly IRAMStorage ramStorage;

        public InMemoryModelService(): this(new TrackDao(), new SubFingerprintDao(), new FingerprintDao(), new SpectralImageDao(), DependencyResolver.Current.Get<IRAMStorage>())
        {
            // no op
        }

        public InMemoryModelService(string loadFrom): this(new TrackDao(), new SubFingerprintDao(), new FingerprintDao(), new SpectralImageDao(), DependencyResolver.Current.Get<IRAMStorage>())
        {
            ramStorage.InitializeFromFile(loadFrom);
        }

        internal InMemoryModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao, ISpectralImageDao spectralImageDao, IRAMStorage ramStorage)
            : base(trackDao, subFingerprintDao, fingerprintDao, spectralImageDao)
        {
            this.ramStorage = ramStorage;
        }

        public override bool SupportsBatchedSubFingerprintQuery
        {
            get
            {
                return false;
            }
        }

        public void Snapshot(string path)
        {
            ramStorage.Snapshot(path);
        }
    }
}
