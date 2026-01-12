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
                [5, 9, 11, 14],
                [trackMatchStartsAt, 5, 9, trackMatchEndsAt]);

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength, trackLength, fingerprintLengthInSeconds, 1d).First();

			Assert.That(coverage.TrackDiscreteCoverageLength, Is.EqualTo(trackMatchEndsAt - trackMatchStartsAt + fingerprintLengthInSeconds).Within(Delta));
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            const double queryLength = 12d;
            const double trackLength = 12d;
            var matches = TestUtilities.GetMatchedWith([1, 2, 3, 4, 5], [1, 2, 9, 11, 12]);

            var coverages = matches.GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength, trackLength, fingerprintLength: 1d, 0d).ToList();

			Assert.That(coverages, Has.Count.EqualTo(2));
			Assert.That(coverages[0].BestPath.Select(_ => _.QuerySequenceNumber), Is.EqualTo(new[] { 3, 4, 5 }).AsCollection);
			Assert.That(coverages[1].BestPath.Select(_ => _.QuerySequenceNumber), Is.EqualTo(new[] { 1, 2 }).AsCollection);
        }

        [Test]
        public void ShouldDisregardJingleSinceTheGapIsTooBig()
        {
            const double queryLength = 5d;
            const double trackLength = 5d;
            var matches = TestUtilities.GetMatchedWith([0, 1, 4, 5, 1, 2], [0, 1, 3, 4, 10, 11]);

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength, trackLength, 1d, 1d).First();

			Assert.That(coverage.TrackDiscreteCoverageLength, Is.EqualTo(5).Within(Delta));
			Assert.That(coverage.BestPath.Select(_ => _.QuerySequenceNumber), Is.EqualTo(new[] { 0, 1, 4, 5 }).AsCollection);
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
            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, seconds + length, shift + seconds + length, 1d / fps, permittedGap: 0d).First();
			Assert.Multiple(() =>
			{
				Assert.That(coverage.TrackDiscreteCoverageLength, Is.EqualTo(seconds).Within(0.0001));
				Assert.That(coverage.TrackCoverageWithPermittedGapsLength, Is.EqualTo(seconds).Within(0.0001));
				Assert.That(coverage.TrackMatchStartsAt, Is.EqualTo(shift * length));
				Assert.That(coverage.QueryMatchStartsAt, Is.EqualTo(0));
			});
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

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength, trackLength, fingerprintLengthInSeconds, permittedGap: 0).First();

			Assert.Multiple(() =>
			{
				Assert.That(coverage.TrackCoverageWithPermittedGapsLength, Is.EqualTo(3 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.TrackDiscreteCoverageLength, Is.EqualTo(5 * fingerprintLengthInSeconds).Within(Delta));

				Assert.That(coverage.QueryGaps.Count(), Is.EqualTo(0));
				Assert.That(coverage.TrackGaps.Count(), Is.EqualTo(2));

				Assert.That(coverage.TrackGaps.First().Start, Is.EqualTo(1 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.TrackGaps.First().End, Is.EqualTo(2 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.TrackGaps.First().LengthInSeconds, Is.EqualTo(fingerprintLengthInSeconds).Within(Delta));

				Assert.That(coverage.TrackGaps.Last().Start, Is.EqualTo(3 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.TrackGaps.Last().End, Is.EqualTo(4 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.TrackGaps.Last().LengthInSeconds, Is.EqualTo(fingerprintLengthInSeconds).Within(Delta));

				Assert.That(coverage.TrackGaps.Sum(d => d.LengthInSeconds), Is.EqualTo(coverage.TrackGapsCoverageLength).Within(Delta));
			});
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

            var coverage = matches.GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength, trackLength, fingerprintLengthInSeconds, permittedGap: 0).First();

			Assert.Multiple(() =>
			{
				Assert.That(coverage.TrackCoverageWithPermittedGapsLength, Is.EqualTo(3 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.TrackDiscreteCoverageLength, Is.EqualTo(3 * fingerprintLengthInSeconds).Within(Delta));

				Assert.That(coverage.TrackGaps.Count(), Is.EqualTo(0));
				Assert.That(coverage.QueryGaps.Count(), Is.EqualTo(2));

				Assert.That(coverage.QueryGaps.First().Start, Is.EqualTo(1 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.QueryGaps.First().End, Is.EqualTo(2 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.QueryGaps.First().LengthInSeconds, Is.EqualTo(fingerprintLengthInSeconds).Within(Delta));

				Assert.That(coverage.QueryGaps.Last().Start, Is.EqualTo(3 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.QueryGaps.Last().End, Is.EqualTo(4 * fingerprintLengthInSeconds).Within(Delta));
				Assert.That(coverage.QueryGaps.Last().LengthInSeconds, Is.EqualTo(fingerprintLengthInSeconds).Within(Delta));
			});
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

            Assert.That(() => entries.FindQueryGaps(endsAt - startsAt, permittedGap, fingerprintLength).ToList(), Throws.Nothing);
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

			Assert.That(trackGaps, Is.Not.Empty);
			Assert.That(trackGaps, Has.Count.EqualTo(1));
            var last = trackGaps.First();

			Assert.Multiple(() =>
			{
				Assert.That(last.Start, Is.EqualTo(totalMatchLength));
				Assert.That(last.LengthInSeconds, Is.EqualTo(prefix));
				Assert.That(last.IsOnEdge, Is.True);
			});
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

			Assert.That(trackDiscontinuities, Has.Count.EqualTo(1));
            var firstGap = trackDiscontinuities.First();

			Assert.Multiple(() =>
			{
				Assert.That(firstGap.IsOnEdge, Is.True);
				Assert.That(firstGap.LengthInSeconds, Is.EqualTo(shift * fingerprintLength));
				Assert.That(firstGap.Start, Is.EqualTo(0));
				Assert.That(firstGap.End, Is.EqualTo(shift * fingerprintLength));

				Assert.That(matchedWiths.FindQueryGaps(count * fingerprintLength, permittedGap, fingerprintLength), Is.Empty);
			});
		}

        [Test]
        public void ShouldIdentifyThatItIsContainedWithinItself()
        {
            // a ------------
            // b    ---

            var a = TestUtilities.GetMatchedWith([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]).GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1).First();
            var b = TestUtilities.GetMatchedWith([4, 5, 6], [4, 5, 6]).GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 3, 3, 1, 1).First();

			Assert.Multiple(() =>
			{
				Assert.That(a.Contains(b), Is.True);
				Assert.That(b.Contains(a), Is.False);
			});

			var results = OverlappingRegionFilter.FilterContainedCoverages([a, b]).ToList();

			Assert.That(results, Has.Count.EqualTo(1));
			Assert.That(results.First(), Is.SameAs(a));
        }

        [Test]
        public void ShouldCalculateConfidenceFromCoverage_1()
        {
            var coverages = new[]
            {
                TestUtilities.GetMatchedWith([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]).GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 0).First(),
                TestUtilities.GetMatchedWith([4, 5, 6], [1, 2, 3]).GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength: 10, trackLength: 3, fingerprintLength: 1, permittedGap: 0).First(),
                TestUtilities.GetMatchedWith([1, 2, 3], [8, 9, 10]).GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength: 3, trackLength: 10, fingerprintLength: 1, permittedGap: 0).First(),
                TestUtilities.GetMatchedWith([10], [1]).GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength: 10, trackLength: 10, fingerprintLength: 1, permittedGap: 0).First(),
                TestUtilities.GetMatchedWith([1], [10]).GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, queryLength: 10, trackLength: 10, fingerprintLength: 1, permittedGap: 0).First()
            };

            foreach (var coverage in coverages)
            {
				Assert.That(coverage.Confidence, Is.EqualTo(1));
            }
        }

        [Test]
        public void ShouldReturnEmptyListForEmptyInput()
        {
            var results = OverlappingRegionFilter.FilterContainedCoverages(Enumerable.Empty<Coverage>()).ToList();

            Assert.That(results, Is.Empty);
        }

        [Test]
        public void ShouldReturnSameCoverageForSingleInput()
        {
            var coverage = TestUtilities.GetMatchedWith([1, 2, 3], [1, 2, 3])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 3, 3, 1, 1)
                .First();

            var results = OverlappingRegionFilter.FilterContainedCoverages([coverage]).ToList();

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results.First(), Is.SameAs(coverage));
        }

        [Test]
        public void ShouldReturnAllCoveragesWhenNoneContained()
        {
            // Create non-overlapping coverages (different track regions)
            // Coverage a: query [1-3], track [1-3]
            // Coverage b: query [10-12], track [10-12]
            var a = TestUtilities.GetMatchedWith([1, 2, 3], [1, 2, 3])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 15, 15, 1, 1)
                .First();
            var b = TestUtilities.GetMatchedWith([10, 11, 12], [10, 11, 12])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 15, 15, 1, 1)
                .First();

            var results = OverlappingRegionFilter.FilterContainedCoverages([a, b]).ToList();

            Assert.That(results, Has.Count.EqualTo(2));
        }

        [Test]
        public void ShouldFilterAllContainedCoveragesWhenOneContainsAll()
        {
            // Large parent coverage containing all smaller ones
            var parent = TestUtilities.GetMatchedWith([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], [1, 2, 3, 4, 5, 6, 7, 8, 9, 10])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();
            var child1 = TestUtilities.GetMatchedWith([2, 3], [2, 3])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();
            var child2 = TestUtilities.GetMatchedWith([5, 6, 7], [5, 6, 7])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();
            var child3 = TestUtilities.GetMatchedWith([8, 9], [8, 9])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();

            var results = OverlappingRegionFilter.FilterContainedCoverages([parent, child1, child2, child3]).ToList();

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results.First(), Is.SameAs(parent));
        }

        [Test]
        public void ShouldHandlePartialOverlapWithoutContainment()
        {
            // Two coverages that overlap but neither contains the other
            // Coverage a: query [1-5], track [1-5]
            // Coverage b: query [3-7], track [3-7]
            var a = TestUtilities.GetMatchedWith([1, 2, 3, 4, 5], [1, 2, 3, 4, 5])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();
            var b = TestUtilities.GetMatchedWith([3, 4, 5, 6, 7], [3, 4, 5, 6, 7])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();

            var results = OverlappingRegionFilter.FilterContainedCoverages([a, b]).ToList();

            Assert.That(results, Has.Count.EqualTo(2));
        }

        [Test]
        public void ShouldHandleQueryContainedButTrackNot()
        {
            // Query dimension: b is within a
            // Track dimension: b is NOT within a
            // Therefore b should NOT be filtered out
            var a = TestUtilities.GetMatchedWith([1, 2, 3, 4, 5], [10, 11, 12, 13, 14])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 20, 1, 1)
                .First();
            var b = TestUtilities.GetMatchedWith([2, 3, 4], [1, 2, 3])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 20, 1, 1)
                .First();

            var results = OverlappingRegionFilter.FilterContainedCoverages([a, b]).ToList();

            Assert.That(results, Has.Count.EqualTo(2));
        }

        [Test]
        public void ShouldFilterTransitivelyCoveredCoverages()
        {
            // a contains b, b contains c
            // Both b and c should be filtered out
            var a = TestUtilities.GetMatchedWith([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], [1, 2, 3, 4, 5, 6, 7, 8, 9, 10])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();
            var b = TestUtilities.GetMatchedWith([3, 4, 5, 6, 7], [3, 4, 5, 6, 7])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();
            var c = TestUtilities.GetMatchedWith([4, 5, 6], [4, 5, 6])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 10, 10, 1, 1)
                .First();

            var results = OverlappingRegionFilter.FilterContainedCoverages([a, b, c]).ToList();

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results.First(), Is.SameAs(a));
        }

        [Test]
        public void ShouldPreserveCoveragesOrderedByLengthInResult()
        {
            // Multiple non-contained coverages should be ordered by coverage length
            var large = TestUtilities.GetMatchedWith([1, 2, 3, 4, 5, 6, 7, 8], [1, 2, 3, 4, 5, 6, 7, 8])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 20, 20, 1, 1)
                .First();
            var medium = TestUtilities.GetMatchedWith([10, 11, 12, 13, 14], [10, 11, 12, 13, 14])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 20, 20, 1, 1)
                .First();
            var small = TestUtilities.GetMatchedWith([17, 18], [17, 18])
                .GetCoverages(QueryPathReconstructionStrategyType.MultipleBestPaths, 20, 20, 1, 1)
                .First();

            // Input in random order
            var results = OverlappingRegionFilter.FilterContainedCoverages([small, large, medium]).ToList();

            Assert.That(results, Has.Count.EqualTo(3));
            // Results should be ordered by TrackDiscreteCoverageLength descending
            Assert.That(results[0].TrackDiscreteCoverageLength, Is.GreaterThanOrEqualTo(results[1].TrackDiscreteCoverageLength));
            Assert.That(results[1].TrackDiscreteCoverageLength, Is.GreaterThanOrEqualTo(results[2].TrackDiscreteCoverageLength));
        }
    }
}