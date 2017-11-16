namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class QueryMathTest
    {
        private readonly QueryMath queryMath = new QueryMath();

        [Test]
        public void ShoulCalculateSnippetLengthCorrectly()
        {
            var hashedFingerprints = new List<HashedFingerprint>
                {
                    new HashedFingerprint(null, 1, 3, Enumerable.Empty<string>()),
                    new HashedFingerprint(null, 0, 1, Enumerable.Empty<string>()),
                    new HashedFingerprint(null, 3, 9.142235f, Enumerable.Empty<string>())
                };

            double snippetLength = queryMath.CalculateExactQueryLength(hashedFingerprints, new DefaultFingerprintConfiguration());

            Assert.AreEqual(9.6284d, snippetLength, 0.0001);
        }

        [Test]
        public void ShouldGetBestCandidatesByHammingDistance()
        {
            var modelService = new Mock<IModelService>(MockBehavior.Strict);
            var trackReference = new ModelReference<int>(3);
            modelService.Setup(s => s.ReadTrackByReference(trackReference)).Returns(
                new TrackData { ISRC = "isrc-1234-1234" });

            var queryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 1 };

            var query = new List<HashedFingerprint>
                {
                    new HashedFingerprint(null, 1, 0, Enumerable.Empty<string>()),
                    new HashedFingerprint(null, 1, 4, Enumerable.Empty<string>()),
                    new HashedFingerprint(null, 1, 8, Enumerable.Empty<string>())
                };

            var first = new ResultEntryAccumulator(query[0], new SubFingerprintData(null, 1, 0, null, null), 100);
            var second = new ResultEntryAccumulator(query[1], new SubFingerprintData(null, 1, 4, null, null), 99);
            var third = new ResultEntryAccumulator(query[2], new SubFingerprintData(null, 1, 8, null, null), 101);
            var hammingSimilarties = new Dictionary<IModelReference, ResultEntryAccumulator>
                {
                    { new ModelReference<int>(1), first },
                    { new ModelReference<int>(2), second },
                    { new ModelReference<int>(3), third },
                };

            var best = queryMath.GetBestCandidates(
                query,
                hammingSimilarties,
                queryConfiguration.MaxTracksToReturn,
                modelService.Object,
                queryConfiguration.FingerprintConfiguration);

            Assert.AreEqual(1, best.Count);
            Assert.AreEqual("isrc-1234-1234", best[0].Track.ISRC);
            Assert.AreEqual(9.48d, best[0].QueryLength, 0.01);
            Assert.AreEqual(0d, best[0].TrackStartsAt);
            modelService.VerifyAll();
        }

        [Test]
        public void ShouldFilterExactMatches0()
        {
            bool result = queryMath.IsCandidatePassingThresholdVotes(
                new HashedFingerprint(new int[] { 1, 2, 3, 4, 5 }, 0, 0, Enumerable.Empty<string>()),
                new SubFingerprintData(new int[] { 1, 2, 3, 7, 8 }, 0, 0, null, null),
                3);

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldFilterExactMatches1()
        {
            bool result = queryMath.IsCandidatePassingThresholdVotes(
                new HashedFingerprint(new int[] { 1, 2, 3, 4, 5 }, 0, 0, Enumerable.Empty<string>()),
                new SubFingerprintData(new int[] { 1, 2, 4, 7, 8 }, 0, 0, null, null),
                3);

            Assert.IsFalse(result);
        }
    }
}
