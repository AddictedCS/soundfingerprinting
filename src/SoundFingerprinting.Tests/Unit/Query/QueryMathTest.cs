namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;

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
                    new HashedFingerprint(null, null, 1, 3d),
                    new HashedFingerprint(null, null, 0, 1d),
                    new HashedFingerprint(null, null, 3, 9.142235)
                };

            double snippetLength = queryMath.CalculateExactQueryLength(hashedFingerprints, new DefaultFingerprintConfiguration());

            Assert.AreEqual(9.6284d, snippetLength, 0.0001);
        }

        [Test]
        public void ShouldGetBestCandidatesByHammingDistance()
        {
            var modelService = new Mock<IModelService>(MockBehavior.Strict);
            var trackReference = new ModelReference<int>(3);
            modelService.Setup(s => s.ReadTrackByReference(trackReference)).Returns(new TrackData { ISRC = "isrc-1234-1234" });

            var queryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 1 };

            var first = new ResultEntryAccumulator(
                new HashedFingerprint(null, null, 1, 0d), new SubFingerprintData(null, 1, 0d, null, null), 100);
            var second = new ResultEntryAccumulator(
                new HashedFingerprint(null, null, 1, 0d), new SubFingerprintData(null, 1, 0d, null, null), 99);
            var third = new ResultEntryAccumulator(
                new HashedFingerprint(null, null, 1, 0d), new SubFingerprintData(null, 1, 0d, null, null), 101);
            var hammingSimilarties = new Dictionary<IModelReference, ResultEntryAccumulator>
                {
                    { new ModelReference<int>(1), first },
                    { new ModelReference<int>(2), second },
                    { new ModelReference<int>(3), third },
                };

          var best = queryMath.GetBestCandidates(new List<HashedFingerprint>(), hammingSimilarties, queryConfiguration.MaxTracksToReturn, modelService.Object, queryConfiguration.FingerprintConfiguration);

          Assert.AreEqual(1, best.Count);
          Assert.AreEqual("isrc-1234-1234", best[0].Track.ISRC);
          modelService.VerifyAll();
        }
    }
}
