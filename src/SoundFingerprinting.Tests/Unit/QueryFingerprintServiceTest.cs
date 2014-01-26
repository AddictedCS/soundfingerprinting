namespace SoundFingerprinting.Tests.Unit
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;

    [TestClass]
    public class QueryFingerprintServiceTest : AbstractTest
    {
        private QueryFingerprintService queryFingerprintService;

        private Mock<IModelService> modelService;

        [TestInitialize]
        public void SetUp()
        {
            modelService = new Mock<IModelService>(MockBehavior.Strict);

            queryFingerprintService = new QueryFingerprintService(modelService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            modelService.VerifyAll();
        }

        [TestMethod]
        public void MaximumNumberOfReturnedTracksIsLessThanAnalyzedCandidatesResultsTest()
        {
            long[] buckets = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var queryHash = new HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 }, buckets);
            const int DefaultThreshold = 5;
            const int FirstTrackId = 20;
            const int SecondTrackId = 21;
            const int FirstSubFingerprintId = 10;
            const int SecondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(FirstTrackId);
            SubFingerprintData firstResult = new SubFingerprintData(
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new ModelReference<int>(FirstSubFingerprintId), firstTrackReference);
            SubFingerprintData secondResult = new SubFingerprintData(
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 10, 12 }, new ModelReference<int>(SecondSubFingerprintId), new ModelReference<int>(SecondTrackId));

            modelService.Setup(service => service.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, DefaultThreshold))
                        .Returns(new List<SubFingerprintData> { firstResult, secondResult });
            modelService.Setup(service => service.ReadTrackByReference(firstTrackReference))
                        .Returns(new TrackData { ISRC = "isrc", TrackReference = firstTrackReference });

            var queryResult = queryFingerprintService.Query(
                new List<HashData> { queryHash }, new CustomQueryConfiguration { MaximumNumberOfTracksToReturnAsResult = 1, ThresholdVotes = DefaultThreshold });

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.ISRC);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(9, queryResult.BestMatch.Similarity);
            Assert.AreEqual(2, queryResult.AnalyzedCandidatesCount);
            Assert.AreEqual(1, queryResult.ResultEntries.Count);
        }

        [TestMethod]
        public void NoResultsReturnedFromUnderlyingStorage()
        {
            var queryHash = new HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 }, new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            modelService.Setup(service => service.ReadSubFingerprintDataByHashBucketsWithThreshold(It.IsAny<long[]>(), 10)).Returns(new List<SubFingerprintData>());

            var queryResult = queryFingerprintService.Query(
                new List<HashData> { queryHash }, new CustomQueryConfiguration { MaximumNumberOfTracksToReturnAsResult = 1, ThresholdVotes = 10 });

            Assert.IsFalse(queryResult.IsSuccessful);
            Assert.IsNull(queryResult.BestMatch);
            Assert.AreEqual(0, queryResult.AnalyzedCandidatesCount);
            Assert.AreEqual(0, queryResult.ResultEntries.Count);
        }
    }
}
