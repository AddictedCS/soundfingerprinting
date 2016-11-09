namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    [TestClass]
    public class QueryMathTest
    {
        private readonly QueryMath queryMath = new QueryMath();

        [TestMethod]
        public void ShoulCalculateSnippetLengthCorrectly()
        {
            var hashedFingerprints = new List<HashedFingerprint>
                {
                    new HashedFingerprint(null, null, 1, 3d),
                    new HashedFingerprint(null, null, 0, 1d),
                    new HashedFingerprint(null, null, 3, 9.142235)
                };

            double snippetLength = queryMath.CalculateExactSnippetLength(hashedFingerprints, new DefaultFingerprintConfiguration());

            Assert.AreEqual(10d, snippetLength, 0.00001);
        }

        [TestMethod]
        public void ShouldGetBestCandidatesByHammingDistance()
        {
            var modelService = new Mock<IModelService>(MockBehavior.Strict);
            var trackReference = new ModelReference<int>(3);
            modelService.Setup(s => s.ReadTrackByReference(trackReference)).Returns(new TrackData { ISRC = "isrc-1234-1234" });

            var queryConfiguration = new QueryConfiguration { MaximumNumberOfTracksToReturnAsResult = 1 };

            var hammingSimilarties = new Dictionary<IModelReference, int>
                {
                    { new ModelReference<int>(1), 100 },
                    { new ModelReference<int>(2), 99 },
                    { new ModelReference<int>(3), 101 },
                };

          var best = queryMath.GetBestCandidates(
                hammingSimilarties, queryConfiguration.MaximumNumberOfTracksToReturnAsResult, modelService.Object);

          Assert.AreEqual(1, best.Count);
          Assert.AreEqual("isrc-1234-1234", best[0].Track.ISRC);
          modelService.VerifyAll();
        }
    }
}
