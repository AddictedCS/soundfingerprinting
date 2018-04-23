namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class MatchedPairTest
    {
        [Test]
        public void ShouldNotRemoveCompetingCandidates()
        {
            var query = new HashedFingerprint(new int[0], 0, 0f, new string[0]);
            var match0 = new MatchedPair(
                query,
                new SubFingerprintData(new int[0], 0, 0f, new ModelReference<int>(0), new ModelReference<int>(0)),
                100);

            var match1 = new MatchedPair(
                query,
                new SubFingerprintData(new int[0], 0, 0f, new ModelReference<int>(1), new ModelReference<int>(1)),
                99);

            var sortedSet = new SortedSet<MatchedPair>(new[] { match0, match1 });

            Assert.AreEqual(2, sortedSet.Count);
        }
    }
}
