namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;
    using System.Linq;

    using DAO;
    using DAO.Data;
    using Infrastructure;

    internal class SpectralImageDao : ISpectralImageDao
    {
        private readonly IRAMStorage ramStorage;

        public SpectralImageDao() : this(DependencyResolver.Current.Get<IRAMStorage>())
        {
        }

        internal SpectralImageDao(IRAMStorage ramStorage)
        {
            this.ramStorage = ramStorage;
        }

        public void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            int orderNumber = 0;
            var dtos = spectralImages.Select(spectralImage => new SpectralImageData(spectralImage, orderNumber++, trackReference)).ToList();
            if (!ramStorage.SpectralImages.ContainsKey(trackReference))
            {
                ramStorage.SpectralImages[trackReference] = dtos;
            }
            else
            {
                ramStorage.SpectralImages[trackReference] =
                    ramStorage.SpectralImages[trackReference].Concat(dtos).ToList();
            }
        }

        public IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference)
        {
            if (ramStorage.SpectralImages.ContainsKey(trackReference))
            {
                return ramStorage.SpectralImages[trackReference];
            }

            return Enumerable.Empty<SpectralImageData>();
        }
    }
}
