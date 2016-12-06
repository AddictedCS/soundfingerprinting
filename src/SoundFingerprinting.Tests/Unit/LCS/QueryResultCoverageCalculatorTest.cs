namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class QueryResultCoverageCalculatorTest
    {
        private readonly QueryResultCoverageCalculator qrc = new QueryResultCoverageCalculator();

        [Test]
        public void ShouldIdentifyLongestMatch()
        {
            var matches = new SortedSet<MatchedPair> 
                {
                    new MatchedPair(new HashedFingerprint(null, null, 10, 5d, new string[0]), new SubFingerprintData(null, 1, 0d, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, null, 20, 9d, new string[0]), new SubFingerprintData(null, 5, 5d, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, null, 30, 11d, new string[0]), new SubFingerprintData(null, 9, 9d, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, null, 40, 14d, new string[0]), new SubFingerprintData(null, 10, 10d, null, null), 100)
                };

            var coverage = qrc.GetCoverage(matches, 10d, new DefaultFingerprintConfiguration());

            Assert.AreEqual(5.4586, coverage.SourceMatchLength, 0.001);
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            var matches = new SortedSet<MatchedPair> 
                {
                    new MatchedPair(new HashedFingerprint(null, null, 10, 5d, new string[0]), new SubFingerprintData(null, 1, 0d, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, null, 20, 9d, new string[0]), new SubFingerprintData(null, 2, 2d, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, null, 30, 11d, new string[0]), new SubFingerprintData(null, 9, 9d, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, null, 40, 14d, new string[0]), new SubFingerprintData(null, 10, 11d, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, null, 40, 14d, new string[0]), new SubFingerprintData(null, 11, 12d, null, null), 100)
                };

            var coverage = qrc.GetCoverage(matches, 5d, new DefaultFingerprintConfiguration());

            Assert.AreEqual(3.9724, coverage.SourceMatchLength, 0.001);
        }
    }
}
