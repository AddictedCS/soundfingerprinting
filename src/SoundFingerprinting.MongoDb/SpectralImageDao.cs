namespace SoundFingerprinting.MongoDb
{
    using System;
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
        internal const string SpectralImage = "SpectralImage";

        public SpectralImageDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
        }

        public void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            var enumerable = spectralImages as List<float[]> ?? spectralImages.ToList();
            for (int orderNumber = 0; orderNumber < enumerable.Count(); orderNumber++)
            {
                var image = new SpectralImage { Image = enumerable[orderNumber], TrackId = ((MongoModelReference)trackReference).Id, OrderNumber = orderNumber };
                var result = GetCollection<SpectralImage>(SpectralImage).Insert(image);
                if (!result.Ok)
                {
                    throw new Exception("Spectral image was not inserted, for track: " + trackReference.Id + " " + result.ErrorMessage);
                }
            }
        }

        public List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference)
        {
            return GetCollection<SpectralImage>(SpectralImage)
                 .AsQueryable()
                 .Where(image => image.TrackId.Equals(trackReference.Id))
                 .Select(GetSpectralImageData).ToList();
        }

        private SpectralImageData GetSpectralImageData(SpectralImage spectralImage)
        {
            return new SpectralImageData(spectralImage.Image, spectralImage.OrderNumber, new MongoModelReference(spectralImage.TrackId));
        }
    }
}
