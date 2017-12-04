namespace SoundFingerprinting.Tests.Unit
{
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
            var queryHash = new HashedFingerprint(GenericHashBuckets(), 1, 0, Enumerable.Empty<string>());
            const int DefaultThreshold = 5;
            const int FirstTrackId = 20;
            const int SecondTrackId = 21;
            const int ThirdTrackId = 22;
            const int FirstSubFingerprintId = 10;
            const int SecondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(FirstTrackId);
            var secondTrackReference = new ModelReference<int>(SecondTrackId);
            var firstResult = new SubFingerprintData(
                GenericHashBuckets(),
                1,
                0,
                new ModelReference<int>(FirstSubFingerprintId),
                firstTrackReference);
            var secondResult = new SubFingerprintData(
                GenericHashBuckets(),
                2,
                0.928f,
                new ModelReference<int>(SecondSubFingerprintId),
                secondTrackReference);
            var thirdResult = new SubFingerprintData(
                GenericHashBuckets(),
                3,
                0.928f * 2,
                new ModelReference<int>(SecondSubFingerprintId),
                new ModelReference<int>(ThirdTrackId));

            var customQueryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 2, ThresholdVotes = DefaultThreshold };
            
            modelService.Setup(service => service.SupportsBatchedSubFingerprintQuery).Returns(false);
            modelService.Setup(
                service => service.ReadSubFingerprints(It.IsAny<int[]>(), customQueryConfiguration)).Returns(
                    new List<SubFingerprintData> { firstResult, secondResult, thirdResult });
            modelService
                .Setup(service => service.ReadTracksByReferences(new[] { firstTrackReference, secondTrackReference }))
                .Returns(new List<TrackData>
                    {
                        new TrackData { ISRC = "isrc", TrackReference = firstTrackReference },
                        new TrackData { ISRC = "isrc_1", TrackReference = secondTrackReference }
                    });

            var queryResult = queryFingerprintService.Query(new List<HashedFingerprint> { queryHash }, customQueryConfiguration, modelService.Object);

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.ISRC);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(100, queryResult.BestMatch.HammingSimilaritySum);
            Assert.AreEqual(2, queryResult.ResultEntries.Count());
            var results = queryResult.ResultEntries.ToList();
            Assert.AreEqual(firstTrackReference, results[0].Track.TrackReference);
            Assert.AreEqual(secondTrackReference,  results[1].Track.TrackReference);
        }

        [Test]
        public void NoResultsReturnedFromUnderlyingStorageTest()
        {
            var queryHash = new HashedFingerprint(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, 0, Enumerable.Empty<string>());
            var customQueryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 1, ThresholdVotes = 10, FingerprintConfiguration = new DefaultFingerprintConfiguration() };
            modelService.Setup(service => service.SupportsBatchedSubFingerprintQuery).Returns(false);
            modelService.Setup(service => service.ReadSubFingerprints(It.IsAny<int[]>(), customQueryConfiguration)).Returns(new List<SubFingerprintData>());

            var queryResult = queryFingerprintService.Query(new List<HashedFingerprint> { queryHash }, customQueryConfiguration, modelService.Object);

            Assert.IsFalse(queryResult.ContainsMatches);
            Assert.IsNull(queryResult.BestMatch);
            Assert.AreEqual(0, queryResult.ResultEntries.Count());
        }

        [Test]
        public void HammingSimilarityIsSummedUpAccrossAllSubFingerprintsTest()
        {
            var queryHash = new HashedFingerprint(GenericHashBuckets(), 0, 0, Enumerable.Empty<string>());
            const int FirstTrackId = 20;
            const int FirstSubFingerprintId = 10;
            const int SecondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(FirstTrackId);
            var firstResult = new SubFingerprintData(GenericHashBuckets(), 1, 0, new ModelReference<int>(FirstSubFingerprintId), firstTrackReference);
            var secondResult = new SubFingerprintData(GenericHashBuckets(), 2, 0.928f, new ModelReference<int>(SecondSubFingerprintId), firstTrackReference);
            var defaultQueryConfiguration = new DefaultQueryConfiguration();

            modelService.Setup(service => service.SupportsBatchedSubFingerprintQuery).Returns(false);
            modelService.Setup(service => service.ReadSubFingerprints(It.IsAny<int[]>(), defaultQueryConfiguration))
                        .Returns(new List<SubFingerprintData> { firstResult, secondResult });
            modelService.Setup(service => service.ReadTracksByReferences(new [] { firstTrackReference }))
                        .Returns(new List<TrackData>{ new TrackData { ISRC = "isrc", TrackReference = firstTrackReference }});

            var queryResult = queryFingerprintService.Query(new List<HashedFingerprint> { queryHash }, defaultQueryConfiguration, modelService.Object);

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual("isrc", queryResult.BestMatch.Track.ISRC);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(200, queryResult.BestMatch.HammingSimilaritySum);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
        }

        [Test]
        public void ShouldSelectProperStrategyAccordingToModelServiceSupportingBatchedQuery()
        {
            modelService.Setup(service => service.SupportsBatchedSubFingerprintQuery).Returns(true);
            modelService.Setup(
                service => service.ReadSubFingerprints(It.IsAny<IEnumerable<int[]>>(), It.IsAny<QueryConfiguration>()))
                .Returns(new HashSet<SubFingerprintData>());

            queryFingerprintService.Query(
                new List<HashedFingerprint>
                    {
                        new HashedFingerprint(GenericHashBuckets(), 0, 0f, Enumerable.Empty<string>())
                    },
                new DefaultQueryConfiguration(),
                modelService.Object);
        }
    }
}
