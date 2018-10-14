namespace SoundFingerprinting.InMemory
{
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Math;

    public class InMemoryModelService : AdvancedModelService
    {
        private readonly IRAMStorage ramStorage;

        public InMemoryModelService() : this(new RAMStorage(25), new StandardGroupingCounter())
        {
        }

        public InMemoryModelService(string loadFrom) : this(new RAMStorage(25), new StandardGroupingCounter())
        {
            ramStorage.InitializeFromFile(loadFrom);
        }

        public InMemoryModelService(IGroupingCounter groupingCounter): this (new RAMStorage(25), groupingCounter)
        {
        }

        private InMemoryModelService(IRAMStorage ramStorage, IGroupingCounter groupingCounter) : this(new TrackDao(ramStorage), new SubFingerprintDao(ramStorage, groupingCounter), new SpectralImageDao(ramStorage), ramStorage)
        {
        }

        private InMemoryModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao, ISpectralImageDao spectralImageDao, IRAMStorage ramStorage) : base(trackDao, subFingerprintDao, spectralImageDao)
        {
            this.ramStorage = ramStorage;
        }

        public void Snapshot(string path)
        {
            ramStorage.Snapshot(path);
        }
    }
}
