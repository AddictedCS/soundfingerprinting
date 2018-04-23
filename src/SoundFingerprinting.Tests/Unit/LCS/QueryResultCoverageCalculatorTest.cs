namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System;
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
                    new MatchedPair(new HashedFingerprint(null, 10, 5, new string[0]), new SubFingerprintData(null, 1, 0, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, 20, 9, new string[0]), new SubFingerprintData(null, 5, 5, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, 30, 11, new string[0]), new SubFingerprintData(null, 9, 9, null, null), 100),
                    new MatchedPair(new HashedFingerprint(null, 40, 14, new string[0]), new SubFingerprintData(null, 10, 10, null, null), 100)
                };

            var coverage = qrc.GetCoverage(matches, 10d, new DefaultFingerprintConfiguration());

            Assert.AreEqual(5.4586, coverage.SourceMatchLength, 0.001);
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            var matches = new SortedSet<MatchedPair>(GetMatchedPairsWithAts(new[] { 1, 2, 3, 4, 5 }, new[] { 1, 2, 9, 11, 12 }));

            var coverage = qrc.GetCoverage(matches, 5d, new DefaultFingerprintConfiguration());

            Assert.AreEqual(4.452d, coverage.SourceMatchLength, 0.001);
        }

        private IEnumerable<MatchedPair> GetMatchedPairsWithAts(int[] queryAt, int[] resultAt)
        {
            if (queryAt.Length != resultAt.Length)
                throw new Exception("Both parameters have to be of the same length");

            List<MatchedPair> pairs = new List<MatchedPair>();
            for (int i = 0; i < queryAt.Length; ++i)
            {
                pairs.Add(GetMatchedPairWithAts(queryAt[i], resultAt[i]));
            }

            return pairs;
        }

        private MatchedPair GetMatchedPairWithAts(int queryAt, int resultAt)
        {
            return new MatchedPair(
                new HashedFingerprint(null, (uint)queryAt, queryAt * 1.48f, new string[0]),
                new SubFingerprintData(null, (uint)resultAt, resultAt * 1.48f, null, null),
                100);
        }
    }
}
