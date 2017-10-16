namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using DAO;

    using InMemory;

    using NUnit.Framework;

    [TestFixture]
    public class SpectralImageDaoTest
    {
        private SpectralImageDao spectralImageDao;

        [SetUp]
        public void SetUp()
        {
            var storage = new RAMStorage(25);
            
            spectralImageDao = new SpectralImageDao(storage);   
        }

        [Test]
        public void ShouldInsertSpectralImages()
        {
            var images = new List<float[]> { new float[0], new float[0], new float[0] };
            var trackReference = new ModelReference<int>(10);

            spectralImageDao.InsertSpectralImages(images, trackReference);

            Assert.AreEqual(3, spectralImageDao.GetSpectralImagesByTrackReference(trackReference).Count());
            var ids = spectralImageDao.GetSpectralImagesByTrackReference(trackReference)
                    .Select(dto => (ulong)dto.SpectralImageReference.Id)
                    .ToList();
            CollectionAssert.AreEqual(Enumerable.Range(1, 3), ids);
        }

        [Test]
        public void ShouldReturnEmptySinceNoSpectralImagesArePresentForTrack()
        {
            var trackReference = new ModelReference<int>(10);

            var results = spectralImageDao.GetSpectralImagesByTrackReference(trackReference);

            Assert.IsEmpty(results);
        }
    }
}
