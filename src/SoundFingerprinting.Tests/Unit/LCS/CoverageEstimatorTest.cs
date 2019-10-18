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
        private const double Delta = 1E-3;

        private readonly float fingerprintLengthInSeconds = (float)new DefaultFingerprintConfiguration().FingerprintLengthInSeconds;
        
        [Test]
        public void ShouldIdentifyLongestMatch()
        {
            const double queryLength = 9d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 5, 9, 11, 14 }, new float[] { 0, 5, 9, 10 });

            var coverage = matches.EstimateCoverage(queryLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(5.4586, coverage.QueryCoverageSeconds, Delta);
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            const double queryLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 1, 2, 3, 4, 5 }, new float[] { 1, 2, 9, 11, 12 });

            var coverage = matches.EstimateCoverage(queryLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(3.9724, coverage.QueryCoverageSeconds, Delta);
            Assert.AreEqual(4.486, coverage.MatchLengthWithTrackDiscontinuities, Delta);
            Assert.AreEqual(-6, coverage.TrackStartsAt);
        }

        [Test]
        public void ShouldDisregardJingleSinceTheGapIsTooBig()
        {
            const double queryLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 1, 4, 5, 1, 2 }, new float[] { 1, 3, 4, 10, 11 });

            var coverage = matches.EstimateCoverage(queryLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(3.9724, coverage.QueryCoverageSeconds, Delta);
            Assert.AreEqual(4.486, coverage.MatchLengthWithTrackDiscontinuities, Delta);
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
            Assert.AreEqual(seconds, coverage.QueryCoverageSeconds, 0.0001);
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

        [Test]
        public void ShouldFindTrackDiscontinuities()
        {
            var count = 3;
            var queryLength = count * fingerprintLengthInSeconds;
            var matches = Enumerable.Range(0, count)
                .Select(i => (uint)i)
                .Select(seqNum => new MatchedWith(
                    querySequenceNumber:     seqNum,
                    queryMatchAt:            seqNum * fingerprintLengthInSeconds,
                    trackSequenceNumber: 2 * seqNum,
                    trackMatchAt:        2 * seqNum * fingerprintLengthInSeconds,
                    score: 100));
            
            var coverage = matches.EstimateCoverage(queryLength, fingerprintLengthInSeconds, permittedGap: 0);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.QueryCoverageSeconds, Delta);
            Assert.AreEqual(5 * fingerprintLengthInSeconds, coverage.MatchLengthWithTrackDiscontinuities, Delta);

            Assert.AreEqual(0, coverage.QueryDiscontinuities.Count());
            Assert.AreEqual(2, coverage.TrackDiscontinuities.Count());

            Assert.AreEqual(1 * fingerprintLengthInSeconds, coverage.TrackDiscontinuities.First().Start, Delta);
            Assert.AreEqual(2 * fingerprintLengthInSeconds, coverage.TrackDiscontinuities.First().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.TrackDiscontinuities.First().LengthInSeconds, Delta);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.TrackDiscontinuities.Last().Start, Delta);
            Assert.AreEqual(4 * fingerprintLengthInSeconds, coverage.TrackDiscontinuities.Last().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.TrackDiscontinuities.Last().LengthInSeconds, Delta);

            Assert.AreEqual(coverage.NotCoveredLength, coverage.TrackDiscontinuities.Sum(d => d.LengthInSeconds), Delta);
        }

        [Test]
        public void ShouldFindQueryDiscontinuities()
        {
            var count = 3;
            var queryLength = count * fingerprintLengthInSeconds;
            var matches = Enumerable.Range(0, count)
                .Select(i => (uint)i)
                .Select(seqNum => new MatchedWith(
                    querySequenceNumber: 2 * seqNum,
                    queryMatchAt:        2 * seqNum * fingerprintLengthInSeconds,
                    trackSequenceNumber:     seqNum,
                    trackMatchAt:            seqNum * fingerprintLengthInSeconds,
                    score: 100));

            var coverage = matches.EstimateCoverage(queryLength, fingerprintLengthInSeconds, permittedGap: 0);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.QueryCoverageSeconds, Delta);
            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.MatchLengthWithTrackDiscontinuities, Delta);

            Assert.AreEqual(0, coverage.TrackDiscontinuities.Count());
            Assert.AreEqual(2, coverage.QueryDiscontinuities.Count());

            Assert.AreEqual(1 * fingerprintLengthInSeconds, coverage.QueryDiscontinuities.First().Start, Delta);
            Assert.AreEqual(2 * fingerprintLengthInSeconds, coverage.QueryDiscontinuities.First().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.QueryDiscontinuities.First().LengthInSeconds, Delta);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.QueryDiscontinuities.Last().Start, Delta);
            Assert.AreEqual(4 * fingerprintLengthInSeconds, coverage.QueryDiscontinuities.Last().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.QueryDiscontinuities.Last().LengthInSeconds, Delta);
        }
    }
}