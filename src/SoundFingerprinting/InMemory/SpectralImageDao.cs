namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;

    internal class SpectralImageDao : ISpectralImageDao
    {
        private readonly IRAMStorage ramStorage;

        public SpectralImageDao(IRAMStorage ramStorage)
        {
            this.ramStorage = ramStorage;
        }

        public void InsertSpectralImages(IEnumerable<SpectralImageData> spectralImages)
        {
            ramStorage.AddSpectralImages(spectralImages);
        }

        public IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference)
        {
            return ramStorage.GetSpectralImagesByTrackReference(trackReference);
        }
    }
}
