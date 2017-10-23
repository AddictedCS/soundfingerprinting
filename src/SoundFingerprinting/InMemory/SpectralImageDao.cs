namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;
    using Infrastructure;

    internal class SpectralImageDao : ISpectralImageDao
    {
        private readonly IRAMStorage ramStorage;

        public SpectralImageDao(IRAMStorage ramStorage)
        {
            this.ramStorage = ramStorage;
        }

        public void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            ramStorage.AddSpectralImages(spectralImages, trackReference);
        }

        public IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference)
        {
            return ramStorage.GetSpectralImagesByTrackReference(trackReference);
        }
    }
}
