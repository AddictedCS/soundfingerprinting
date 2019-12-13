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
            const double trackLength = 9d;
            const float trackMatchStartsAt = 0f;
            const float trackMatchEndsAt = 10;
            var matches = TestUtilities.GetMatchedWith(new[] { 5f, 9, 11, 14 }, new[] { trackMatchStartsAt, 5, 9, trackMatchEndsAt });

            var coverage = matches.EstimateCoverage(queryLength, trackLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(trackMatchEndsAt - trackMatchStartsAt + fingerprintLengthInSeconds, coverage.DiscreteCoverageLength, Delta);
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            const double queryLength = 5d;
            const double trackLength = 5d;
            const float trackMatchStartsAt = 9f;
            var matches = TestUtilities.GetMatchedWith(new[] { 1f, 2, 3, 4, 5 }, new[] { 1, 2, trackMatchStartsAt, 11, 12 });

            var coverage = matches.EstimateCoverage(queryLength, trackLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(4.486, coverage.DiscreteCoverageLength, Delta);
            Assert.AreEqual(-6, coverage.TrackStartsAt);
        }

        [Test]
        public void ShouldDisregardJingleSinceTheGapIsTooBig()
        {
            const double queryLength = 5d;
            const double trackLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new[] { 1f, 4, 5, 1, 2 }, new[] { 1f, 3, 4, 10, 11 });

            var coverage = matches.EstimateCoverage(queryLength, trackLength, fingerprintLengthInSeconds, 1d);

            Assert.AreEqual(4.486, coverage.DiscreteCoverageLength, Delta);
        }

        [Test]
        public void ShouldCalculateCoverageCorrectlyForImageSearch()
        {
            int fps = 30;
            int seconds = 10;
            float[] queryMatchAt = new float[fps * seconds];
            float[] dbMatchAt = new float[fps * seconds];
            float shift = 11.5f;
            float length = 1f / fps;
            for (int i = 0; i < fps * seconds; ++i)
            {
                queryMatchAt[i] = i * 1f / fps;
                dbMatchAt[i] = shift + i * 1f / fps;
            }

            var matches = TestUtilities.GetMatchedWith(queryMatchAt, dbMatchAt);
            var coverage = matches.EstimateCoverage(seconds + length, shift + seconds + length, 1d / fps, 1d);
            Assert.AreEqual(seconds, coverage.DiscreteCoverageLength, 0.0001);
            Assert.AreEqual(seconds, coverage.CoverageLength, 0.0001);
            Assert.AreEqual(shift, coverage.TrackMatchStartsAt);
            Assert.AreEqual(0, coverage.QueryMatchStartsAt);
        }

        [Test]
        public void BestPathShouldIdentifyBestShiftingMatchesByScore()
        {
            var all = new List<MatchedWith>();
            var count = 100;
            for (int trackSequence = 0; trackSequence < count; ++trackSequence)
            {
                for (int querySequence = trackSequence; querySequence < trackSequence + count; ++querySequence)
                {
                    var match = new MatchedWith((uint)querySequence, querySequence * 1.48f, (uint)trackSequence, trackSequence * 1.48f, score: querySequence);
                    all.Add(match);
                }
            }

            var coverage = all.EstimateCoverage(count * 1.48, count * 1.48, 1.48, 1.48);

            // shifted matches, best path
            var bestPath = coverage.BestPath.ToList();
            int shift = (count - 1);
            for (int i = 0; i < bestPath.Count; ++i)
            {
                Assert.AreEqual(shift + i, bestPath[i].QuerySequenceNumber);
                Assert.AreEqual(i, bestPath[i].TrackSequenceNumber);
            }
        }

        [Test]
        public void ShouldFindTrackDiscontinuities()
        {
            var count = 3;
            var queryLength = count * fingerprintLengthInSeconds;
            var trackLength = count * fingerprintLengthInSeconds;
            var matches = Enumerable.Range(0, count)
                .Select(i => (uint)i)
                .Select(seqNum => new MatchedWith(
                    querySequenceNumber: seqNum,
                    queryMatchAt: seqNum * fingerprintLengthInSeconds,
                    trackSequenceNumber: 2 * seqNum,
                    trackMatchAt: 2 * seqNum * fingerprintLengthInSeconds,
                    score: 100));

            var coverage = matches.EstimateCoverage(queryLength, trackLength, fingerprintLengthInSeconds, permittedGap: 0);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.CoverageLength, Delta);
            Assert.AreEqual(5 * fingerprintLengthInSeconds, coverage.DiscreteCoverageLength, Delta);

            Assert.AreEqual(0, coverage.QueryGaps.Count());
            Assert.AreEqual(2, coverage.TrackGaps.Count());

            Assert.AreEqual(1 * fingerprintLengthInSeconds, coverage.TrackGaps.First().Start, Delta);
            Assert.AreEqual(2 * fingerprintLengthInSeconds, coverage.TrackGaps.First().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.TrackGaps.First().LengthInSeconds, Delta);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.TrackGaps.Last().Start, Delta);
            Assert.AreEqual(4 * fingerprintLengthInSeconds, coverage.TrackGaps.Last().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.TrackGaps.Last().LengthInSeconds, Delta);

            Assert.AreEqual(coverage.GapsCoverageLength, coverage.TrackGaps.Sum(d => d.LengthInSeconds), Delta);
        }

        [Test]
        public void ShouldFindQueryDiscontinuities()
        {
            var count = 3;
            var queryLength = count * fingerprintLengthInSeconds;
            var trackLength = count * fingerprintLengthInSeconds;
            var matches = Enumerable.Range(0, count)
                .Select(i => (uint)i)
                .Select(seqNum => new MatchedWith(
                    querySequenceNumber: 2 * seqNum,
                    queryMatchAt: 2 * seqNum * fingerprintLengthInSeconds,
                    trackSequenceNumber: seqNum,
                    trackMatchAt: seqNum * fingerprintLengthInSeconds,
                    score: 100));

            var coverage = matches.EstimateCoverage(queryLength, trackLength, fingerprintLengthInSeconds, permittedGap: 0);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.CoverageLength, Delta);
            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.DiscreteCoverageLength, Delta);

            Assert.AreEqual(0, coverage.TrackGaps.Count());
            Assert.AreEqual(2, coverage.QueryGaps.Count());

            Assert.AreEqual(1 * fingerprintLengthInSeconds, coverage.QueryGaps.First().Start, Delta);
            Assert.AreEqual(2 * fingerprintLengthInSeconds, coverage.QueryGaps.First().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.QueryGaps.First().LengthInSeconds, Delta);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.QueryGaps.Last().Start, Delta);
            Assert.AreEqual(4 * fingerprintLengthInSeconds, coverage.QueryGaps.Last().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.QueryGaps.Last().LengthInSeconds, Delta);
        }

        [Test]
        public void FindGapsFloatingPointEdgeCase()
        {
            const double permittedGap = 0;
            const double fingerprintLength = 8192 / 5512d;

            uint startsAtSeqNum = 20;
            float startsAt = 3.71552968025207519531250000000f;
            var start = new MatchedWith(startsAtSeqNum, startsAt, 0, 0, 0);

            uint endsAtSeqNum = 28;
            float endsAt = 5.20174169540405273437500000000f;
            var end = new MatchedWith(endsAtSeqNum, endsAt, 0, 0, 0);

            var entries = new[] { start, end };

            Assert.DoesNotThrow(() => entries.FindQueryGaps(permittedGap, fingerprintLength).ToList());
        }

        [Test]
        public void ShouldIdentifyGapAtTheEnd()
        {
            int count = 10;
            double fingerprintLength = 0.5;
            double totalMatchLength = count * fingerprintLength;
            double prefix = 2;
            double trackLength = totalMatchLength + prefix;
            var matchedWiths = Enumerable.Range(0, count)
                .Select(index => new MatchedWith((uint)index, (float)(index * fingerprintLength), (uint) index, (float)(index * fingerprintLength), 15))
                .ToList();

            var trackGaps= matchedWiths.FindTrackGaps(trackLength, 1, fingerprintLength)
                .ToList();
            
            Assert.IsNotEmpty(trackGaps);
            Assert.AreEqual(1, trackGaps.Count);
            var last = trackGaps.First();
            
            Assert.AreEqual(totalMatchLength, last.Start);
            Assert.AreEqual(prefix, last.LengthInSeconds);
            Assert.IsTrue(last.IsOnEdge);
        }
        
        [Test]
        public void ShouldIdentifyGapAtTheBeginning()
        {
            int shift = 3;
            int count = 10;
            double fingerprintLength = 0.5, permittedGap = 1, trackLength= 5;
            var matchedWiths = Enumerable.Range(0, count)
                .Select(index =>
                {
                    uint trackSequenceNumber = (uint) (shift + index);
                    float trackMatchAt = (float)(trackSequenceNumber * fingerprintLength);
                    return new MatchedWith((uint) index, (float)(index * fingerprintLength), trackSequenceNumber, trackMatchAt, 15);
                })
                .ToList();

            var trackDiscontinuities = matchedWiths
                .FindTrackGaps(trackLength, permittedGap, fingerprintLength)
                .ToList();
            
            Assert.AreEqual(1, trackDiscontinuities.Count);
            var firstGap = trackDiscontinuities.First();
            
            Assert.IsTrue(firstGap.IsOnEdge);
            Assert.AreEqual(shift * fingerprintLength, firstGap.LengthInSeconds);
            Assert.AreEqual(0, firstGap.Start);
            Assert.AreEqual(shift * fingerprintLength, firstGap.End);
            
            Assert.IsEmpty(matchedWiths.FindQueryGaps(permittedGap, fingerprintLength));
        }
    }
}