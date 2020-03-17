namespace SoundFingerprinting.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    [TestFixture]
    public class QueryFingerprintServiceTest : AbstractTest
    {
        private readonly QueryFingerprintService queryFingerprintService = QueryFingerprintService.Instance;

        private Mock<IModelService> modelService;

        [SetUp]
        public void SetUp()
        {
            modelService = new Mock<IModelService>(MockBehavior.Strict);
        }

        [TearDown]
        public void TearDown()
        {
            modelService.VerifyAll();
        }

        [Test]
        public void MaximumNumberOfReturnedTracksIsLessThanAnalyzedCandidatesResultsTest()
        {
            var queryHash = new HashedFingerprint(GenericHashBuckets(), 1, 0);
            const int defaultThreshold = 5;
            const int firstTrackId = 20;
            const int secondTrackId = 21;
            const int thirdTrackId = 22;
            const int firstSubFingerprintId = 10;
            const int secondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(firstTrackId);
            var secondTrackReference = new ModelReference<int>(secondTrackId);
            var firstResult = new SubFingerprintData(GenericHashBuckets(), 1, 0,
                new ModelReference<int>(firstSubFingerprintId),
                firstTrackReference);
            var secondResult = new SubFingerprintData(GenericHashBuckets(), 2, 0.928f,
                new ModelReference<int>(secondSubFingerprintId),
                secondTrackReference);
            var thirdResult = new SubFingerprintData(GenericHashBuckets(), 3, 0.928f * 2,
                new ModelReference<int>(secondSubFingerprintId),
                new ModelReference<int>(thirdTrackId));

            var customQueryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 2, ThresholdVotes = defaultThreshold };

            modelService
                .Setup(service => service.Query(It.IsAny<IEnumerable<int[]>>(), customQueryConfiguration))
                .Returns(new[] { firstResult, secondResult, thirdResult });

            modelService.Setup(service => service.ReadTracksByReferences(new[] { firstTrackReference, secondTrackReference }))
                .Returns(new List<TrackData>
                    {
                        new TrackData("isrc", string.Empty, string.Empty, string.Empty, 0, 0d, firstTrackReference),
                        new TrackData("isrc_1", string.Empty, string.Empty, string.Empty, 0, 0d, secondTrackReference)
                    });

            var queryResult = queryFingerprintService.Query(new List<HashedFingerprint> { queryHash }, customQueryConfiguration, DateTime.Now, modelService.Object);

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.Id);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(100, queryResult.BestMatch.Score);
            Assert.AreEqual(2, queryResult.ResultEntries.Count());
            var results = queryResult.ResultEntries.ToList();
            Assert.AreEqual(firstTrackReference, results[0].Track.TrackReference);
            Assert.AreEqual(secondTrackReference,  results[1].Track.TrackReference);
        }

        [Test]
        public void NoResultsReturnedFromUnderlyingStorageTest()
        {
            var queryHash = new HashedFingerprint(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, 0);
            var customQueryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 1, ThresholdVotes = 10, FingerprintConfiguration = new DefaultFingerprintConfiguration() };
            modelService.Setup(service => service.Query(It.IsAny<IEnumerable<int[]>>(), customQueryConfiguration))
                .Returns(new List<SubFingerprintData>());

            var queryResult = queryFingerprintService.Query(new List<HashedFingerprint> { queryHash }, customQueryConfiguration, DateTime.Now, modelService.Object);

            Assert.IsFalse(queryResult.ContainsMatches);
            Assert.IsNull(queryResult.BestMatch);
            Assert.AreEqual(0, queryResult.ResultEntries.Count());
        }

        [Test]
        public void HammingSimilarityIsSummedUpAcrossAllSubFingerprintsTest()
        {
            var queryHash = new HashedFingerprint(GenericHashBuckets(), 0, 0);
            const int firstTrackId = 20;
            const int firstSubFingerprintId = 10;
            const int secondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(firstTrackId);
            var firstResult = new SubFingerprintData(GenericHashBuckets(), 1, 0, new ModelReference<int>(firstSubFingerprintId), firstTrackReference);
            var secondResult = new SubFingerprintData(GenericHashBuckets(), 2, 0.928f, new ModelReference<int>(secondSubFingerprintId), firstTrackReference);
            var defaultQueryConfiguration = new DefaultQueryConfiguration();

            modelService.Setup(service => service.Query(
                        It.IsAny<IEnumerable<int[]>>(),
                        It.IsAny<QueryConfiguration>())).Returns(new[] { firstResult, secondResult });

            modelService.Setup(service => service.ReadTracksByReferences(new[] { firstTrackReference })).Returns(
                new List<TrackData>
                    {
                        new TrackData(
                            "isrc",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            0,
                            0d,
                            firstTrackReference)
                    });

            var queryResult = queryFingerprintService.Query(new List<HashedFingerprint> { queryHash }, defaultQueryConfiguration, DateTime.Now, modelService.Object);

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.Id);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(200, queryResult.BestMatch.Score);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
        }

        [Test]
        public void ShouldSelectProperStrategyAccordingToModelServiceSupportingBatchedQuery()
        {
            modelService.Setup(service => service.Query(It.IsAny<IEnumerable<int[]>>(), It.IsAny<QueryConfiguration>()))
                .Returns(new List<SubFingerprintData>());

            queryFingerprintService.Query(
                new List<HashedFingerprint>
                    {
                        new HashedFingerprint(GenericHashBuckets(), 0, 0f)
                    },
                new DefaultQueryConfiguration(),
                DateTime.Now,
                modelService.Object);
        }
    }
}
