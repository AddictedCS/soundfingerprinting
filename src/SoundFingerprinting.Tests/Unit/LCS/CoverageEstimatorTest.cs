namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Linq;

    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.LCS;
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
            const int trackMatchStartsAt = 0;
            const int trackMatchEndsAt = 10;
            var matches = TestUtilities.GetMatchedWith(
                new[] { 5, 9, 11, 14 }, 
                new[] { trackMatchStartsAt, 5, 9, trackMatchEndsAt });

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength, trackLength, fingerprintLengthInSeconds, 1d).First();

            Assert.AreEqual(trackMatchEndsAt - trackMatchStartsAt + fingerprintLengthInSeconds, coverage.TrackDiscreteCoverageLength, Delta);
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            const double queryLength = 5d;
            const double trackLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new[] { 1, 2, 3, 4, 5 }, new[] { 1, 2, 9, 11, 12 });

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength, trackLength, fingerprintLength: 1d, 0d).First();

            Assert.AreEqual(12, coverage.TrackDiscreteCoverageLength, Delta);
            Assert.AreEqual(8, coverage.TrackGapsCoverageLength);
            Assert.AreEqual(0, coverage.TrackStartsAt);
            CollectionAssert.AreEqual(new [] {1, 2, 3, 4, 5}, coverage.BestPath.Select(_ => _.QuerySequenceNumber));
        }

        [Test]
        public void ShouldDisregardJingleSinceTheGapIsTooBig()
        {
            const double queryLength = 5d;
            const double trackLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new[] { 0, 1, 4, 5, 1, 2 }, new[] { 0, 1, 3, 4, 10, 11 });

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength, trackLength, 1d, 1d).First();

            Assert.AreEqual(5, coverage.TrackDiscreteCoverageLength, Delta);
            CollectionAssert.AreEqual(new [] {0, 1, 4, 5}, coverage.BestPath.Select(_ => _.QuerySequenceNumber));
        }

        [Test]
        public void ShouldCalculateCoverageCorrectlyForImageSearch()
        {
            int fps = 30;
            int seconds = 10;
            int[] queryMatchAt = new int[fps * seconds];
            int[] dbMatchAt = new int[fps * seconds];
            int shift = 1000;
            float length = 1f / fps;
            for (int i = 0; i < fps * seconds; ++i)
            {
                queryMatchAt[i] = i;
                dbMatchAt[i] = shift + i;
            }

            var matches = TestUtilities.GetMatchedWith(queryMatchAt, dbMatchAt, 100, length);
            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, seconds + length, shift + seconds + length, 1d / fps, permittedGap: 0d).First();
            Assert.AreEqual(seconds, coverage.TrackDiscreteCoverageLength, 0.0001);
            Assert.AreEqual(seconds, coverage.TrackCoverageWithPermittedGapsLength, 0.0001);
            Assert.AreEqual(shift * length, coverage.TrackMatchStartsAt);
            Assert.AreEqual(0, coverage.QueryMatchStartsAt);
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

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength, trackLength, fingerprintLengthInSeconds, permittedGap: 0).First();

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(5 * fingerprintLengthInSeconds, coverage.TrackDiscreteCoverageLength, Delta);

            Assert.AreEqual(0, coverage.QueryGaps.Count());
            Assert.AreEqual(2, coverage.TrackGaps.Count());

            Assert.AreEqual(1 * fingerprintLengthInSeconds, coverage.TrackGaps.First().Start, Delta);
            Assert.AreEqual(2 * fingerprintLengthInSeconds, coverage.TrackGaps.First().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.TrackGaps.First().LengthInSeconds, Delta);

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.TrackGaps.Last().Start, Delta);
            Assert.AreEqual(4 * fingerprintLengthInSeconds, coverage.TrackGaps.Last().End, Delta);
            Assert.AreEqual(fingerprintLengthInSeconds, coverage.TrackGaps.Last().LengthInSeconds, Delta);

            Assert.AreEqual(coverage.TrackGapsCoverageLength, coverage.TrackGaps.Sum(d => d.LengthInSeconds), Delta);
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

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength, trackLength, fingerprintLengthInSeconds, permittedGap: 0).First();

            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(3 * fingerprintLengthInSeconds, coverage.TrackDiscreteCoverageLength, Delta);

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

            Assert.DoesNotThrow(() => entries.FindQueryGaps(endsAt - startsAt, permittedGap, fingerprintLength).ToList());
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
            
            Assert.IsEmpty(matchedWiths.FindQueryGaps(count * fingerprintLength, permittedGap, fingerprintLength));
        }

        [Test]
        public void ShouldIdentifyThatItIsContainedWithinItself()
        {
            // a ------------
            // b    ---

            var a = TestUtilities.GetMatchedWith(new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}).GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, 10, 10, 1, 1).First();
            var b = TestUtilities.GetMatchedWith(new[] {4, 5, 6}, new[] {4, 5, 6}).GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, 3, 3, 1, 1).First();

            Assert.IsTrue(a.Contains(b));
            Assert.IsFalse(b.Contains(a));

            var results = OverlappingRegionFilter.FilterContainedCoverages(new[] {a, b}).ToList();
            
            Assert.AreEqual(1, results.Count);
            Assert.AreSame(a, results.First());
        }

        [Test]
        public void ShouldCalculateConfidenceFromCoverage_1()
        {
            var coverages = new[]
            {
                TestUtilities.GetMatchedWith(new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}).GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, 10, 10, 1, 0).First(),
                TestUtilities.GetMatchedWith(new[] {4, 5, 6}, new[] {1, 2, 3}).GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength: 10, trackLength: 3, fingerprintLength: 1, permittedGap: 0).First(),
                TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {8, 9, 10}).GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength: 3, trackLength: 10, fingerprintLength: 1, permittedGap: 0).First(),
                TestUtilities.GetMatchedWith(new[] {10}, new[] {1}).GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength: 10, trackLength: 10, fingerprintLength: 1, permittedGap: 0).First(),
                TestUtilities.GetMatchedWith(new[] {1}, new[] {10}).GetCoverages(QueryPathReconstructionStrategyType.SingleBestPath, queryLength: 10, trackLength: 10, fingerprintLength: 1, permittedGap: 0).First()
            };

            foreach (var coverage in coverages)
            { 
                Assert.AreEqual(1, coverage.Confidence);
            }
        }
    }
}