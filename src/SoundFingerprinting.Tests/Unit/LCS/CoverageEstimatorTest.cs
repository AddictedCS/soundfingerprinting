namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class CoverageEstimatorTest
    {
        private readonly double fingerprintLengthInSeconds = new DefaultFingerprintConfiguration().FingerprintLengthInSeconds;
        
        [Test]
        public void ShouldIdentifyLongestMatch()
        {
            const double queryLength = 9d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 5, 9, 11, 14 }, new float[] { 0, 5, 9, 10 });

            var coverage = matches.EstimateCoverage(queryLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(5.4586, coverage.QueryCoverageSum, 0.001);
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            const double queryLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 1, 2, 3, 4, 5 }, new float[] { 1, 2, 9, 11, 12 });

            var coverage = matches.EstimateCoverage(queryLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(3.9724, coverage.QueryCoverageSum, 0.01);
            Assert.AreEqual(4.486, coverage.MatchLengthWithTrackDiscontinuities, 0.01);
            Assert.AreEqual(-6, coverage.TrackStartsAt);
        }

        [Test]
        public void ShouldDisregardJingleSinceTheGapIsTooBig()
        {
            const double queryLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 1, 4, 5, 1, 2 }, new float[] { 1, 3, 4, 10, 11 });

            var coverage = matches.EstimateCoverage(queryLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(3.9724, coverage.QueryCoverageSum, 0.01);
            Assert.AreEqual(4.486, coverage.MatchLengthWithTrackDiscontinuities, 0.01);
        }

        [Test]
        public void ShouldCalculateCoverageCorrectlyForImageSearch()
        {
            int fps = 30;
            int seconds = 10;
            float[] queryMatchAt = new float[fps * seconds];
            float[] dbMatchAt = new float[fps * seconds];
            float shift = 11.5f;
            for (int i = 0; i < fps * seconds; ++i)
            {
                queryMatchAt[i] = i * 1f / fps;
                dbMatchAt[i] = shift + i * 1f / fps;
            }
            
            var matches = TestUtilities.GetMatchedWith(queryMatchAt, dbMatchAt);
            var coverage = matches.EstimateCoverage(seconds, 1d / fps, 1d);
            
            Assert.AreEqual(seconds, coverage.MatchLengthWithTrackDiscontinuities, 0.0001);
            Assert.AreEqual(seconds, coverage.QueryCoverageSum, 0.0001);
            Assert.AreEqual(shift, coverage.TrackMatchStartsAt);
            Assert.AreEqual(0, coverage.QueryMatchStartsAt);
        }

        [Test]
        public void BestPathShouldIdentifyBestShiftingMatchesByScore()
        {
            var all = new List<MatchedWith>();
            var count = 100;
            for (int querySequence = 0; querySequence < count; ++querySequence)
            {
                for (int trackSequence = querySequence; trackSequence < querySequence + count; ++trackSequence)
                {
                    var match = new MatchedWith((uint)querySequence, querySequence * 1.48f, (uint)trackSequence, trackSequence * 1.48f, trackSequence);
                    all.Add(match);
                }
            }

            var coverage = all.EstimateCoverage(count * 1.48, 1.48, 1.48);

            // shifted matches, best path
            var bestPath = coverage.BestPath.ToList();
            int shift = (count - 1);
            for (int i = 0; i < bestPath.Count; ++i)
            {
                Assert.AreEqual(shift + i, bestPath[i].TrackSequenceNumber);
                Assert.AreEqual(i, bestPath[i].QuerySequenceNumber);
            }
        }
    }
}