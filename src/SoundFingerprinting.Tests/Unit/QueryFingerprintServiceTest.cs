namespace SoundFingerprinting.Tests.Unit
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
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
            long[] buckets = GenericHashBuckets;
            var queryHash = new HashedFingerprint(GenericSignature, buckets, 1, 0);
            const int DefaultThreshold = 5;
            const int FirstTrackId = 20;
            const int SecondTrackId = 21;
            const int ThirdTrackId = 22;
            const int FirstSubFingerprintId = 10;
            const int SecondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(FirstTrackId);
            var secondTrackReference = new ModelReference<int>(SecondTrackId);
            var thirdTrackReference = new ModelReference<int>(ThirdTrackId);
            var firstResult = new SubFingerprintData(
                GenericHashBuckets,
                1,
                0,
                new ModelReference<int>(FirstSubFingerprintId),
                firstTrackReference);
            var secondResult = new SubFingerprintData(
                GenericHashBuckets,
                2,
                0.928,
                new ModelReference<int>(SecondSubFingerprintId),
                secondTrackReference);
            var thirdResult = new SubFingerprintData(
                GenericHashBuckets,
                3,
                0.928 * 2,
                new ModelReference<int>(SecondSubFingerprintId),
                new ModelReference<int>(ThirdTrackId));

            var customQueryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 3, ThresholdVotes = DefaultThreshold };
            modelService.Setup(
                service => service.ReadSubFingerprints(buckets, customQueryConfiguration)).Returns(
                    new List<SubFingerprintData> { firstResult, secondResult, thirdResult });
            modelService.Setup(service => service.ReadTrackByReference(firstTrackReference)).Returns(
                new TrackData { ISRC = "isrc", TrackReference = firstTrackReference });
            modelService.Setup(service => service.ReadTrackByReference(secondTrackReference)).Returns(
               new TrackData { ISRC = "isrc_1", TrackReference = secondTrackReference });
            modelService.Setup(service => service.ReadTrackByReference(thirdTrackReference)).Returns(
                new TrackData { ISRC = "isrc_2", TrackReference = thirdTrackReference });

            var queryResult = queryFingerprintService.Query(modelService.Object, new List<HashedFingerprint> { queryHash }, customQueryConfiguration);

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.ISRC);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(50, queryResult.BestMatch.HammingSimilarity);
            Assert.AreEqual(3, queryResult.AnalyzedTracksCount);
            Assert.AreEqual(3, queryResult.ResultEntries.Count);
            Assert.AreEqual(firstTrackReference, queryResult.ResultEntries[0].Track.TrackReference);
            Assert.AreEqual(secondTrackReference, queryResult.ResultEntries[1].Track.TrackReference);
            Assert.AreEqual(thirdTrackReference, queryResult.ResultEntries[2].Track.TrackReference);
        }

        [TestMethod]
        public void NoResultsReturnedFromUnderlyingStorageTest()
        {
            var queryHash = new HashedFingerprint(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 }, new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, 0);
            var customQueryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 1, ThresholdVotes = 10, FingerprintConfiguration = new DefaultFingerprintConfiguration() };
            modelService.Setup(service => service.ReadSubFingerprints(It.IsAny<long[]>(), customQueryConfiguration)).Returns(new List<SubFingerprintData>());

            var queryResult = queryFingerprintService.Query(
                modelService.Object,
                new List<HashedFingerprint> { queryHash },
                customQueryConfiguration);

            Assert.IsFalse(queryResult.IsSuccessful);
            Assert.IsNull(queryResult.BestMatch);
            Assert.AreEqual(0, queryResult.AnalyzedTracksCount);
            Assert.AreEqual(0, queryResult.ResultEntries.Count);
        }

        [TestMethod]
        public void HammingSimilarityIsSummedUpAccrossAllSubFingerprintsTest()
        {
            long[] buckets = GenericHashBuckets;
            var queryHash = new HashedFingerprint(GenericSignature, buckets, 0, 0);
            const int FirstTrackId = 20;
            const int FirstSubFingerprintId = 10;
            const int SecondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(FirstTrackId);
            var firstResult = new SubFingerprintData(GenericHashBuckets, 1, 0, new ModelReference<int>(FirstSubFingerprintId), firstTrackReference);
            var secondResult = new SubFingerprintData(GenericHashBuckets, 2, 0.928, new ModelReference<int>(SecondSubFingerprintId), firstTrackReference);
            var defaultQueryConfiguration = new DefaultQueryConfiguration(); 

            modelService.Setup(service => service.ReadSubFingerprints(buckets, defaultQueryConfiguration))
                        .Returns(new List<SubFingerprintData> { firstResult, secondResult });
            modelService.Setup(service => service.ReadTrackByReference(firstTrackReference))
                        .Returns(new TrackData { ISRC = "isrc", TrackReference = firstTrackReference });

            var queryResult = queryFingerprintService.Query(
                modelService.Object,
                new List<HashedFingerprint> { queryHash },
                defaultQueryConfiguration);

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.ISRC);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(GenericSignature.Length * 2, queryResult.BestMatch.HammingSimilarity);
            Assert.AreEqual(1, queryResult.AnalyzedTracksCount);
            Assert.AreEqual(1, queryResult.ResultEntries.Count);
        }
    }
}
