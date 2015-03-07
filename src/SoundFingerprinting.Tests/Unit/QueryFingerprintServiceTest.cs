namespace SoundFingerprinting.Tests.Unit
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
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

            queryFingerprintService = new QueryFingerprintService();
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
            const int ThirdTrackId = 22;
            const int FirstSubFingerprintId = 10;
            const int SecondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(FirstTrackId);
            var thirdTrackReference = new ModelReference<int>(ThirdTrackId);
            SubFingerprintData firstResult = new SubFingerprintData(
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                1,
                new ModelReference<int>(FirstSubFingerprintId),
                firstTrackReference);
            SubFingerprintData secondResult = new SubFingerprintData(
                new byte[] { 11, 2, 13, 4, 15, 6, 7, 8, 10, 12 },
                2,
                new ModelReference<int>(SecondSubFingerprintId),
                new ModelReference<int>(SecondTrackId));
            SubFingerprintData thirdResult = new SubFingerprintData(
                new byte[] { 1, 2, 3, 4, 5, 15, 7, 8, 10, 12 },
                3,
                new ModelReference<int>(SecondSubFingerprintId),
                new ModelReference<int>(ThirdTrackId));

            modelService.Setup(
                service => service.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, DefaultThreshold)).Returns(
                    new List<SubFingerprintData> { firstResult, secondResult, thirdResult });
            modelService.Setup(service => service.ReadTrackByReference(firstTrackReference)).Returns(
                new TrackData { ISRC = "isrc", TrackReference = firstTrackReference });
            modelService.Setup(service => service.ReadTrackByReference(thirdTrackReference)).Returns(
                new TrackData { ISRC = "isrc_2", TrackReference = thirdTrackReference });

            var queryResult = queryFingerprintService.Query2(
                modelService.Object,
                new List<HashData> { queryHash },
                new CustomQueryConfiguration
                    {
                        MaximumNumberOfTracksToReturnAsResult = 2, ThresholdVotes = DefaultThreshold 
                    });

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.ISRC);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(9, queryResult.BestMatch.Similarity);
            Assert.AreEqual(3, queryResult.AnalyzedCandidatesCount);
            Assert.AreEqual(2, queryResult.ResultEntries.Count);
            Assert.AreEqual(firstTrackReference, queryResult.ResultEntries[0].Track.TrackReference);
            Assert.AreEqual(thirdTrackReference, queryResult.ResultEntries[1].Track.TrackReference);
        }

        [TestMethod]
        public void NoResultsReturnedFromUnderlyingStorageTest()
        {
            var queryHash = new HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 }, new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            modelService.Setup(service => service.ReadSubFingerprintDataByHashBucketsWithThreshold(It.IsAny<long[]>(), 10)).Returns(new List<SubFingerprintData>());

            var queryResult = queryFingerprintService.Query2(
                modelService.Object,
                new List<HashData> { queryHash },
                new CustomQueryConfiguration { MaximumNumberOfTracksToReturnAsResult = 1, ThresholdVotes = 10 });

            Assert.IsFalse(queryResult.IsSuccessful);
            Assert.IsNull(queryResult.BestMatch);
            Assert.AreEqual(0, queryResult.AnalyzedCandidatesCount);
            Assert.AreEqual(0, queryResult.ResultEntries.Count);
        }

        [TestMethod]
        public void HammingSimilarityIsSummedUpAccrossAllSubFingerprintsTest()
        {
            long[] buckets = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var queryHash = new HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 }, buckets);
            const int DefaultThreshold = 5;
            const int FirstTrackId = 20;
            const int FirstSubFingerprintId = 10;
            const int SecondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(FirstTrackId);
            SubFingerprintData firstResult = new SubFingerprintData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 1, new ModelReference<int>(FirstSubFingerprintId), firstTrackReference);
            SubFingerprintData secondResult = new SubFingerprintData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 10, 12 }, 2, new ModelReference<int>(SecondSubFingerprintId), firstTrackReference);

            modelService.Setup(service => service.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, DefaultThreshold))
                        .Returns(new List<SubFingerprintData> { firstResult, secondResult });
            modelService.Setup(service => service.ReadTrackByReference(firstTrackReference))
                        .Returns(new TrackData { ISRC = "isrc", TrackReference = firstTrackReference });

            var queryResult = queryFingerprintService.Query2(modelService.Object, new List<HashData> { queryHash }, new DefaultQueryConfiguration());

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.ISRC);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(9 + 8, queryResult.BestMatch.Similarity);
            Assert.AreEqual(1, queryResult.AnalyzedCandidatesCount);
            Assert.AreEqual(1, queryResult.ResultEntries.Count);
        }

        [TestMethod]
        public void OnlyTracksWithGroupIdAreConsideredAsPotentialCandidatesTest()
        {
            long[] buckets = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var queryHash = new HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 }, buckets);
            const int DefaultThreshold = 5;
            const int FirstTrackId = 20;
            const int FirstSubFingerprintId = 10;
            var firstTrackReference = new ModelReference<int>(FirstTrackId);
            SubFingerprintData firstResult = new SubFingerprintData(
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                1,
                new ModelReference<int>(FirstSubFingerprintId),
                firstTrackReference);
      
            modelService.Setup(
                service =>
                service.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(buckets, DefaultThreshold, "group-id"))
                .Returns(new List<SubFingerprintData> { firstResult });
            modelService.Setup(service => service.ReadTrackByReference(firstTrackReference)).Returns(
                new TrackData { ISRC = "isrc", TrackReference = firstTrackReference });

            var queryResult = queryFingerprintService.Query2(
                modelService.Object,
                new List<HashData> { queryHash },
                new CustomQueryConfiguration { TrackGroupId = "group-id" });

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.ISRC);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
        }
    }
}
