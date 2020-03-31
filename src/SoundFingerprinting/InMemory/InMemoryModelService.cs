namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
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

        public InMemoryModelService(IRAMStorage ramStorage, IGroupingCounter groupingCounter) : this(new TrackDao(ramStorage), new SubFingerprintDao(ramStorage, groupingCounter), new SpectralImageDao(ramStorage), ramStorage)
        {
        }

        private InMemoryModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao, ISpectralImageDao spectralImageDao, IRAMStorage ramStorage) : base("in-memory-model-service", trackDao, subFingerprintDao, spectralImageDao)
        {
            this.ramStorage = ramStorage;
        }

        public void Snapshot(string path)
        {
            ramStorage.Snapshot(path);
        }

        public override IEnumerable<TrackData> ReadTrackByTitle(string title)
        {
            return ramStorage.Tracks
                .Where(pair => pair.Value.Title == title)
                .Select(pair => pair.Value);
        }
    }
}
