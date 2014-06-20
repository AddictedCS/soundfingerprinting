namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;

    using MongoDB.Driver.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.DAO;
    using SoundFingerprinting.MongoDb.Entity;

    internal class SpectralImageDao : AbstractDao, ISpectralImageDao
    {
        private const string SpectralImage = "SpectralImage";

        public SpectralImageDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
        }

        public void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            var imagesToInsert = new List<SpectralImage>();
            var enumerable = spectralImages as List<float[]> ?? spectralImages.ToList();
            for (int i = 0; i < enumerable.Count(); i++)
            {
                var image = new SpectralImage { Image = enumerable[i], TrackId = ((MongoModelReference)trackReference).Id, OrderNumber = i };
                imagesToInsert.Add(image);
            }

            GetCollection<SpectralImage>(SpectralImage).InsertBatch(imagesToInsert);
        }

        public List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference)
        {
            return GetCollection<SpectralImage>(SpectralImage)
                                    .AsQueryable()
                                    .Where(image => image.TrackId.Equals(trackReference.Id))
                                    .Select(image => GetSpectralImageData(image)).ToList();
        }

        private SpectralImageData GetSpectralImageData(SpectralImage spectralImage)
        {
            return new SpectralImageData(spectralImage.Image, spectralImage.OrderNumber, new MongoModelReference(spectralImage.TrackId));
        }
    }
}
