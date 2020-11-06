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
            var queryHash = new HashedFingerprint(GenericHashBuckets(), 1, 0, Array.Empty<byte>());
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
                .Setup(service => service.Query(It.IsAny<Hashes>(), customQueryConfiguration))
                .Returns(new[] { firstResult, secondResult, thirdResult });

            modelService.Setup(service => service.ReadTracksByReferences(new[] { firstTrackReference, secondTrackReference }))
                .Returns(new List<TrackData>
                    {
                        new TrackData("id", string.Empty, string.Empty, 0d, firstTrackReference),
                        new TrackData("id_1", string.Empty, string.Empty, 0d, secondTrackReference)
                    });

            var hashes = new Hashes(new List<HashedFingerprint> { queryHash }, 1.48f, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>());
            var queryResult = queryFingerprintService.Query(hashes, customQueryConfiguration, modelService.Object);

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual("id", queryResult.BestMatch.Track.Id);
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
            var queryHash = new HashedFingerprint(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, 0, Array.Empty<byte>());
            var customQueryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 1, ThresholdVotes = 10, FingerprintConfiguration = new DefaultFingerprintConfiguration() };
            modelService.Setup(service => service.Query(It.IsAny<Hashes>(), customQueryConfiguration)).Returns(new List<SubFingerprintData>());

            var hashes = new Hashes(new List<HashedFingerprint> { queryHash }, 148f, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>());
            var queryResult = queryFingerprintService.Query(hashes, customQueryConfiguration, modelService.Object);

            Assert.IsFalse(queryResult.ContainsMatches);
            Assert.IsNull(queryResult.BestMatch);
        }

        [Test]
        public void HammingSimilarityIsSummedUpAcrossAllSubFingerprintsTest()
        {
            var queryHash = new HashedFingerprint(GenericHashBuckets(), 0, 0, Array.Empty<byte>());
            const int firstTrackId = 20;
            const int firstSubFingerprintId = 10;
            const int secondSubFingerprintId = 11;
            var firstTrackReference = new ModelReference<int>(firstTrackId);
            var firstResult = new SubFingerprintData(GenericHashBuckets(), 1, 0, new ModelReference<int>(firstSubFingerprintId), firstTrackReference);
            var secondResult = new SubFingerprintData(GenericHashBuckets(), 2, 0.928f, new ModelReference<int>(secondSubFingerprintId), firstTrackReference);
            var defaultQueryConfiguration = new DefaultQueryConfiguration();

            modelService.Setup(service => service.Query(
                        It.IsAny<Hashes>(),
                        It.IsAny<QueryConfiguration>())).Returns(new[] { firstResult, secondResult });

            modelService.Setup(service => service.ReadTracksByReferences(new[] { firstTrackReference })).Returns(
                new List<TrackData>
                    {
                        new TrackData("id", string.Empty, string.Empty, 0d, firstTrackReference)
                    });

            var hashes = new Hashes(new List<HashedFingerprint> { queryHash }, 1.48f, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>());
            var queryResult = queryFingerprintService.Query(hashes, defaultQueryConfiguration, modelService.Object);

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual("id", queryResult.BestMatch.Track.Id);
            Assert.AreEqual(firstTrackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.AreEqual(200, queryResult.BestMatch.Score);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
        }

        [Test]
        public void ShouldSelectProperStrategyAccordingToModelServiceSupportingBatchedQuery()
        {
            modelService.Setup(service => service.Query(It.IsAny<Hashes>(), It.IsAny<QueryConfiguration>()))
                .Returns(new List<SubFingerprintData>());

            queryFingerprintService.Query(new Hashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>())
                }, 1.48f, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()),
                new DefaultQueryConfiguration(),
                modelService.Object);
        }
    }
}
