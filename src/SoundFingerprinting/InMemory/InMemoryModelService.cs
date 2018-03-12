namespace SoundFingerprinting.InMemory
{
    using SoundFingerprinting.DAO;

    public class InMemoryModelService : AdvancedModelService
    {
        private readonly IRAMStorage ramStorage;

        public InMemoryModelService(): this(new RAMStorage(50))
        {
        }

        public InMemoryModelService(string loadFrom): this(new RAMStorage(50))
        {
            ramStorage.InitializeFromFile(loadFrom);
        }

        internal InMemoryModelService(IRAMStorage ramStorage)
            : this(
                new TrackDao(ramStorage),
                new SubFingerprintDao(ramStorage),
                new SpectralImageDao(ramStorage),
                ramStorage)
        {
        }

        private InMemoryModelService(
            ITrackDao trackDao,
            ISubFingerprintDao subFingerprintDao,
            ISpectralImageDao spectralImageDao,
            IRAMStorage ramStorage)
            : base(trackDao, subFingerprintDao, spectralImageDao)
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
