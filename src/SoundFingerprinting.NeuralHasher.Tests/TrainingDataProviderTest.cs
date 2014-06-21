namespace SoundFingerprinting.NeuralHasher.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Tests;

    [TestClass]
    public class TrainingDataProviderTest : AbstractTest
    {
        private Mock<IModelService> modelService;

        private TrainingDataProvider trainingDataProvider;

        [TestInitialize]
        public void SetUp()
        {
            modelService = new Mock<IModelService>(MockBehavior.Strict);
            trainingDataProvider = new TrainingDataProvider(modelService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            modelService.VerifyAll();
        }

        [TestMethod]
        public void TestCorrectNumberOfSpectralImagesIsGatheredFromTheDataSource()
        {
            const int NumberOfTracks = 2;
            var tracks = GetSampleTracks(NumberOfTracks);
            const int NumberOfSpectralImages = 10;
            var images = GetSpectralImagesForTracks(tracks, NumberOfSpectralImages);
            modelService.Setup(service => service.ReadAllTracks()).Returns(tracks);
            modelService.Setup(
                service =>
                service.GetSpectralImagesByTrackId(It.Is<ModelReference<int>>(reference => reference.Id == 0))).Returns(
                images[0]);
            modelService.Setup(
                service =>
                service.GetSpectralImagesByTrackId(It.Is<ModelReference<int>>(reference => reference.Id == 1))).Returns(
                images[1]);

            var trainingData = trainingDataProvider.GetSpectralImagesToTrain(
                new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, NumberOfTracks);

            Assert.AreEqual(NumberOfTracks, trainingData.Count);
            Assert.AreEqual(NumberOfSpectralImages, trainingData[tracks[0].TrackReference].Length);
            Assert.AreEqual(NumberOfSpectralImages, trainingData[tracks[1].TrackReference].Length);
            for (int trackIndex = 0; trackIndex < NumberOfTracks; trackIndex++)
            {
                for (int i = 0; i < NumberOfSpectralImages; i++)
                {
                    Assert.AreEqual(i, trainingData[tracks[trackIndex].TrackReference][i][0]);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestNotEnoughTracksInTheTrainingDataSource()
        {
            var tracks = new List<TrackData> { new TrackData(), new TrackData() };
            modelService.Setup(service => service.ReadAllTracks()).Returns(tracks);

            trainingDataProvider.GetSpectralImagesToTrain(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestDataSourceContainsTracksThatAreNotSuitableForTrainingData()
        {
            const int NumberOfTracks = 2;
            var tracks = GetSampleTracks(NumberOfTracks);
            const int NumberOfSpectralImages = 10;
            var images = GetSpectralImagesForTracks(tracks, NumberOfSpectralImages - 1);
            modelService.Setup(service => service.ReadAllTracks()).Returns(tracks);
            modelService.Setup(
                service =>
                service.GetSpectralImagesByTrackId(It.Is<ModelReference<int>>(reference => reference.Id == 0))).Returns(
                    images[0]);

            trainingDataProvider.GetSpectralImagesToTrain(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 2);
        }

        private List<List<SpectralImageData>> GetSpectralImagesForTracks(IEnumerable<TrackData> tracks, int numberOfSpectralImagesPerTrack)
        {
            var images = new List<List<SpectralImageData>>();
            foreach (var track in tracks)
            {
                var imagesPerTrack = new List<SpectralImageData>();
                for (int spectralImageIndex = 0; spectralImageIndex < numberOfSpectralImagesPerTrack; spectralImageIndex++)
                {
                    var spectralImage = new SpectralImageData(new float[] { spectralImageIndex }, spectralImageIndex, track.TrackReference);
                    imagesPerTrack.Add(spectralImage);
                }

                images.Add(imagesPerTrack);
            }

            return images;
        }

        private List<TrackData> GetSampleTracks(int numberOfTracks)
        {
            var tracks = new List<TrackData>();
            for (int i = 0; i < numberOfTracks; i++)
            {
                var track = new TrackData("isrc", "artist", "title", "album", 0, 0)
                    { TrackReference = new ModelReference<int>(i) };
                tracks.Add(track);
            }

            return tracks;
        }
    }
}
