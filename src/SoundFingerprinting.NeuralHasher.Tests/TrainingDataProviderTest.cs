namespace SoundFingerprinting.NeuralHasher.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.NeuralHasher.Utils;
    using SoundFingerprinting.Tests;

    [TestClass]
    public class TrainingDataProviderTest : AbstractTest
    {
        private Mock<IModelService> modelService;
        private Mock<IBinaryOutputHelper> binaryOutputHelper;

        private TrainingDataProvider trainingDataProvider;

        [TestInitialize]
        public void SetUp()
        {
            modelService = new Mock<IModelService>(MockBehavior.Strict);
            binaryOutputHelper = new Mock<IBinaryOutputHelper>(MockBehavior.Strict);
            trainingDataProvider = new TrainingDataProvider(modelService.Object, binaryOutputHelper.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            modelService.VerifyAll();
            binaryOutputHelper.VerifyAll();
        }

        [TestMethod]
        public void TestCorrectNumberOfSpectralImagesIsGatheredFromTheDataSource()
        {
            const int NumberOfTracks = 2;
            var tracks = GetSampleTracks(NumberOfTracks);
            const int NumberOfSpectralImages = 10;
            var images = GetSpectralImagesForTracks(tracks, NumberOfSpectralImages);
            modelService.Setup(service => service.ReadAllTracks()).Returns(tracks);
            modelService.Setup(s => s.GetSpectralImagesByTrackId(It.Is<ModelReference<int>>(r => r.Id == 0))).Returns(
                images[0]);
            modelService.Setup(s => s.GetSpectralImagesByTrackId(It.Is<ModelReference<int>>(r => r.Id == 1))).Returns(
                images[1]);

            var trainingData = trainingDataProvider.GetSpectralImagesToTrain(
                new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, NumberOfTracks);

            Assert.AreEqual(NumberOfTracks, trainingData.Count);
            Assert.AreEqual(NumberOfSpectralImages, trainingData[0].Length);
            Assert.AreEqual(NumberOfSpectralImages, trainingData[1].Length);
            for (int trackIndex = 0; trackIndex < NumberOfTracks; trackIndex++)
            {
                for (int spectralImageIndex = 0; spectralImageIndex < NumberOfSpectralImages; spectralImageIndex++)
                {
                    Assert.AreEqual(spectralImageIndex, trainingData[trackIndex][spectralImageIndex][0]);
                }
            }
        }

        [TestMethod]
        public void TestSpectralImagesAreMappedCorrectlyToBinaryCodes()
        {
            var firstTrack = new[] { new double[] { 1, 1 }, new double[] { 2, 2 } };
            var secondTrack = new[] { new double[] { 3, 3 }, new double[] { 4, 4 } };
            binaryOutputHelper.Setup(helper => helper.GetBinaryCodes(1)).Returns(new[] { new byte[] { 0 }, new byte[] { 1 } });

            TrainingSet set = trainingDataProvider.MapSpectralImagesToBinaryOutputs(new List<double[][]> { firstTrack, secondTrack }, 1);

            Assert.AreEqual(4, set.Inputs.Length);
            Assert.AreEqual(4, set.Outputs.Length);
            AssertArraysAreEqual(new double[] { 1, 1 }, set.Inputs[0]);
            AssertArraysAreEqual(new double[] { 2, 2 }, set.Inputs[1]);
            AssertArraysAreEqual(new double[] { 3, 3 }, set.Inputs[2]);
            AssertArraysAreEqual(new double[] { 4, 4 }, set.Inputs[3]);
            AssertArraysAreEqual(new double[] { 0 }, set.Outputs[0]);
            AssertArraysAreEqual(new double[] { 0 }, set.Outputs[1]);
            AssertArraysAreEqual(new double[] { 1 }, set.Outputs[2]);
            AssertArraysAreEqual(new double[] { 1 }, set.Outputs[3]);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestNotEnoughTracksInTheTrainingDataSource()
        {
            var tracks = GetSampleTracks(2);

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
