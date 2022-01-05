namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class ResultEntryConcatenatorTest
    {
        private const double Delta = 1E-3;

        [Test]
        public void ReturnNullWhenBothEntriesAreNull()
        {
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            Assert.IsNull(concatenator.Concat(null, null));
        }

        [Test]
        public void WhenOneEntryIsNullThenReturnTheOther()
        {
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var entry = CreateEntry(queryOffset: 110, trackOffset: 0, matchLength: 10);

            Assert.AreSame(entry, concatenator.Concat(entry, null));
            Assert.AreSame(entry, concatenator.Concat(null, entry));
        }

        [Test]
        public void ShouldNotConcatEntriesFromDifferentTracks()
        {
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var left = CreateEntry(queryOffset: 110, trackOffset: 0, matchLength: 10, trackLength: 30, queryLength: 120, trackId: "track-1");
            var right = CreateEntry(queryOffset: 0, trackOffset: 10, matchLength: 20, trackLength: 30, queryLength: 120, trackId: "track-2");

            Assert.Throws<ArgumentException>(() => concatenator.Concat(left, right));
        }

        [Test]
        public void NoGaps()
        {
            /*
             * a:   [  0 ... 109 110 111 112 113 114 115 116 117 118 119]
             *                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * b:                                                       [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19 ... 119]
             *                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * track:           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29]
             */
            var left  = CreateEntry(queryOffset: 110, trackOffset:  0, matchLength: 10);
            var right = CreateEntry(queryOffset:   0, trackOffset: 10, matchLength: 20);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.IsNotNull(concatenated);
            Assert.IsEmpty(concatenated.Coverage.TrackGaps);
            Assert.IsEmpty(concatenated.Coverage.QueryGaps);

            Assert.AreEqual(1, concatenated.TrackRelativeCoverage, Delta);
            Assert.AreEqual(30, concatenated.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(30, concatenated.DiscreteTrackCoverageLength, Delta);
            Assert.AreEqual(240, concatenated.QueryLength, Delta);
            Assert.AreEqual(110, concatenated.QueryMatchStartsAt, Delta);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt, Delta);
            Assert.AreEqual(110, concatenated.TrackStartsAt, Delta);
            Assert.AreEqual(0, concatenated.Coverage.TrackGapsCoverageLength);
            Assert.AreEqual(1, concatenated.Confidence, 0.05);
            Assert.IsTrue(concatenated.NoGaps);
        }

        [Test]
        public void TrackGapBetweenTheQueries()
        {
            /*
             * a:   [  0 ... 110 111 112 113 114 115 116 117 118 119]
             *                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * b:                                                               [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18 ... 119]
             *                                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * track:           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29]
             */
            var left  = CreateEntry(queryOffset: 110, trackOffset:  0, matchLength:  9);
            var right = CreateEntry(queryOffset:   0, trackOffset: 12, matchLength: 18);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.IsNotNull(concatenated);
            Assert.AreEqual(1, concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(9, 12, concatenated.Coverage.TrackGaps.First());
            Assert.IsEmpty(concatenated.Coverage.QueryGaps);

            Assert.AreEqual(0.9, concatenated.TrackRelativeCoverage, Delta);
            Assert.AreEqual(1, TrackDiscreteCoverage(concatenated.Coverage), Delta);
            Assert.AreEqual(27, concatenated.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(30, concatenated.DiscreteTrackCoverageLength, Delta);
            Assert.AreEqual(240, concatenated.QueryLength, Delta);
            Assert.AreEqual(110, concatenated.QueryMatchStartsAt, Delta);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt, Delta);
            Assert.AreEqual(110, concatenated.TrackStartsAt, Delta);
            Assert.AreEqual(3, concatenated.Coverage.TrackGapsCoverageLength);
            Assert.AreEqual(0.9, concatenated.Confidence, 0.05);
            Assert.IsFalse(concatenated.NoGaps);
        }

        [Test]
        public void QueryGapAtTheEndOfTheFirstQuery()
        {
            /*
             * a:   [  0 ... 106 107 108 109 110 111 112 113 114 115 116 117 118 119]
             *                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * b:                                                       [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19 ... 119]
             *                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * track:           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29]
             */
            var left  = CreateEntry(queryOffset: 107, trackOffset:  0, matchLength: 10);
            var right = CreateEntry(queryOffset:   0, trackOffset: 10, matchLength: 20);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.IsNotNull(concatenated);
            Assert.IsEmpty(concatenated.Coverage.TrackGaps);
            Assert.AreEqual(1, concatenated.Coverage.QueryGaps.Count());
            AssertDiscontinuity(117, 120, concatenated.Coverage.QueryGaps.First());

            Assert.AreEqual(1, concatenated.TrackRelativeCoverage, Delta);
            Assert.AreEqual(1, TrackDiscreteCoverage(concatenated.Coverage), Delta);
            Assert.AreEqual(30, concatenated.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(30, concatenated.DiscreteTrackCoverageLength, Delta);
            Assert.AreEqual(240, concatenated.QueryLength, Delta);
            Assert.AreEqual(107, concatenated.QueryMatchStartsAt, Delta);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt, Delta);
            Assert.AreEqual(107, concatenated.TrackStartsAt, Delta);
            Assert.AreEqual(0, concatenated.Coverage.TrackGapsCoverageLength);
            Assert.AreEqual(1, concatenated.Confidence, 0.05);
            Assert.IsFalse(concatenated.NoGaps);
        }

        [Test]
        public void QueryGapAtTheStartOfTheSecondQuery()
        {
            /*
             * a:   [  0 ... 109 110 111 112 113 114 115 116 117 118 119]
             *                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * b:                                           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23 ... 119]
             *                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * track:           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29]
             */
            var left  = CreateEntry(queryOffset: 110, trackOffset:  0, matchLength: 10);
            var right = CreateEntry(queryOffset:   3, trackOffset: 10, matchLength: 20);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.IsNotNull(concatenated);
            Assert.IsEmpty(concatenated.Coverage.TrackGaps);
            Assert.AreEqual(1, concatenated.Coverage.QueryGaps.Count());
            AssertDiscontinuity(120, 123, concatenated.Coverage.QueryGaps.First());

            Assert.AreEqual(1, concatenated.TrackRelativeCoverage, Delta);
            Assert.AreEqual(1, TrackDiscreteCoverage(concatenated.Coverage), Delta);
            Assert.AreEqual(30, concatenated.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(30, concatenated.DiscreteTrackCoverageLength, Delta);
            Assert.AreEqual(240, concatenated.QueryLength, Delta);
            Assert.AreEqual(110, concatenated.QueryMatchStartsAt, Delta);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt, Delta);
            Assert.AreEqual(110, concatenated.TrackStartsAt, Delta);
            Assert.AreEqual(0, concatenated.Coverage.TrackGapsCoverageLength);
            Assert.AreEqual(1, concatenated.Confidence, 0.05);
            Assert.IsFalse(concatenated.NoGaps);
        }

        [Test]
        public void QueryGapsAroundTheStitch()
        {
            /*
             * a:   [  0 ... 107 108 109 110 111 112 113 114 115 116 117 118 119]
             *                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * b:                                               [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22 ... 119]
             *                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * track:           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29]
             */
            var left  = CreateEntry(queryOffset: 108, trackOffset:  0, matchLength: 10);
            var right = CreateEntry(queryOffset:   2, trackOffset: 10, matchLength: 20);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.IsNotNull(concatenated);
            Assert.IsEmpty(concatenated.Coverage.TrackGaps);
            Assert.AreEqual(1, concatenated.Coverage.QueryGaps.Count());
            AssertDiscontinuity(118, 122, concatenated.Coverage.QueryGaps.First());

            Assert.AreEqual(1, concatenated.TrackRelativeCoverage, Delta);
            Assert.AreEqual(1, TrackDiscreteCoverage(concatenated.Coverage), Delta);
            Assert.AreEqual(30, concatenated.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(30, concatenated.DiscreteTrackCoverageLength, Delta);
            Assert.AreEqual(240, concatenated.QueryLength, Delta);
            Assert.AreEqual(108, concatenated.QueryMatchStartsAt, Delta);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt, Delta);
            Assert.AreEqual(108, concatenated.TrackStartsAt, Delta);
            Assert.AreEqual(0, concatenated.Coverage.TrackGapsCoverageLength);
            Assert.AreEqual(1, concatenated.Confidence, 0.05);
            Assert.IsFalse(concatenated.NoGaps);
        }

        [Test]
        public void QueryAndTrackGapsAtTheEndOfTheFirstQuery()
        {
            /*
             * a:   [  0 ... 109 110 111 112 113 114 115 116 117 118 119]
             *                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * b:                                                       [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19 ... 119]
             *                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * track:           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29]
             */
            var left  = CreateEntry(queryOffset: 110, trackOffset:  0, matchLength:  7);
            var right = CreateEntry(queryOffset:   0, trackOffset: 10, matchLength: 20);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.IsNotNull(concatenated);
            Assert.AreEqual(1, concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(7, 10, concatenated.Coverage.TrackGaps.First());
            Assert.AreEqual(1, concatenated.Coverage.QueryGaps.Count());
            AssertDiscontinuity(117, 120, concatenated.Coverage.QueryGaps.First());

            Assert.AreEqual(0.9, concatenated.TrackRelativeCoverage, Delta);
            Assert.AreEqual(1, TrackDiscreteCoverage(concatenated.Coverage), Delta);
            Assert.AreEqual(27, concatenated.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(30, concatenated.DiscreteTrackCoverageLength, Delta);
            Assert.AreEqual(240, concatenated.QueryLength, Delta);
            Assert.AreEqual(110, concatenated.QueryMatchStartsAt, Delta);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt, Delta);
            Assert.AreEqual(110, concatenated.TrackStartsAt, Delta);
            Assert.AreEqual(3, concatenated.Coverage.TrackGapsCoverageLength);
            Assert.AreEqual(0.9, concatenated.Confidence, 0.05);
            Assert.IsFalse(concatenated.NoGaps);
        }

        [Test]
        public void QueryAndTrackGapsAtTheStartOfTheSecondQuery()
        {
            /*
             * a:   [  0 ... 109 110 111 112 113 114 115 116 117 118 119]
             *                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * b:                                                       [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19 ... 119]
             *                                                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * track:           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29]
             */
            var left  = CreateEntry(queryOffset: 110, trackOffset:  0, matchLength: 10);
            var right = CreateEntry(queryOffset:   3, trackOffset: 13, matchLength: 17);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.IsNotNull(concatenated);
            Assert.AreEqual(1, concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(10, 13, concatenated.Coverage.TrackGaps.First());
            Assert.AreEqual(1, concatenated.Coverage.QueryGaps.Count());
            AssertDiscontinuity(120, 123, concatenated.Coverage.QueryGaps.First());

            Assert.AreEqual(0.9, concatenated.TrackRelativeCoverage, Delta);
            Assert.AreEqual(1, TrackDiscreteCoverage(concatenated.Coverage), Delta);
            Assert.AreEqual(27, concatenated.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(30, concatenated.DiscreteTrackCoverageLength, Delta);
            Assert.AreEqual(240, concatenated.QueryLength, Delta);
            Assert.AreEqual(110, concatenated.QueryMatchStartsAt, Delta);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt, Delta);
            Assert.AreEqual(110, concatenated.TrackStartsAt, Delta);
            Assert.AreEqual(3, concatenated.Coverage.TrackGapsCoverageLength);
            Assert.AreEqual(0.9, concatenated.Confidence, 0.05);
            Assert.IsFalse(concatenated.NoGaps);
        }

        [Test]
        public void QueryAndTrackGapsAroundTheStitch()
        {
            /*
             * a:   [  0 ... 107 108 109 110 111 112 113 114 115 116 117 118 119]
             *                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * b:                                               [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22 ... 119]
             *                                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             * track:           [  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29]
             */
            var left  = CreateEntry(queryOffset: 108, trackOffset:  0, matchLength:  9);
            var right = CreateEntry(queryOffset:   4, trackOffset: 12, matchLength: 18);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.IsNotNull(concatenated);
            Assert.AreEqual(1, concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(9, 12, concatenated.Coverage.TrackGaps.First());
            Assert.AreEqual(1, concatenated.Coverage.QueryGaps.Count());
            AssertDiscontinuity(117, 124, concatenated.Coverage.QueryGaps.First());

            Assert.AreEqual(0.9, concatenated.TrackRelativeCoverage, Delta);
            Assert.AreEqual(1, TrackDiscreteCoverage(concatenated.Coverage), Delta);
            Assert.AreEqual(27, concatenated.TrackCoverageWithPermittedGapsLength, Delta);
            Assert.AreEqual(30, concatenated.DiscreteTrackCoverageLength, Delta);
            Assert.AreEqual(240, concatenated.QueryLength, Delta);
            Assert.AreEqual(108, concatenated.QueryMatchStartsAt, Delta);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt, Delta);
            Assert.AreEqual(108, concatenated.TrackStartsAt, Delta);
            Assert.AreEqual(3, concatenated.Coverage.TrackGapsCoverageLength);
            Assert.AreEqual(0.9, concatenated.Confidence, 0.05);
            Assert.IsFalse(concatenated.NoGaps);
        }

        [Test]
        public void ShouldStitchMatchThatHappensOnTheEdge()
        {
            var first  = CreateEntry(queryOffset: 5, trackOffset:  0, matchLength: 5, trackLength: 10, queryLength: 10); 
            var second  = CreateEntry(queryOffset: 0, trackOffset:  5, matchLength: 5, trackLength: 10, queryLength: 10); 
            
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(first, second);
            
            Assert.AreEqual(1, concatenated.TrackRelativeCoverage, 0.01);
            Assert.AreEqual(0.5, concatenated.QueryRelativeCoverage, 0.01);
            Assert.AreEqual(20, concatenated.QueryLength);
            Assert.AreEqual(5, concatenated.QueryMatchStartsAt);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt);
            Assert.AreEqual(10, concatenated.TrackCoverageWithPermittedGapsLength, 0.01);
        }
        
        [Test]
        public void ShouldStitchTwoFullMatches()
        {
            int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, trackOffset:  0, matchLength, trackLength: 20, queryLength: 10);
            var second = CreateEntry(queryOffset: 0, trackOffset: 10, matchLength, trackLength: 20, queryLength: 10);
            
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(first, second);
            
            Assert.AreEqual(1, concatenated.TrackRelativeCoverage, 0.01);
            Assert.AreEqual(1, concatenated.QueryRelativeCoverage, 0.01);
            Assert.AreEqual(20, concatenated.QueryLength);
            Assert.AreEqual(0, concatenated.QueryMatchStartsAt);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt);

            var bestPath = concatenated.Coverage.BestPath.ToList();
            for (int i = 1; i < bestPath.Count; ++i)
            {
                var current = bestPath[i];
                var prev = bestPath[i - 1];
                Assert.AreEqual(0.1, current.QueryMatchAt - prev.QueryMatchAt, 0.00001);
                Assert.AreEqual(1, current.QuerySequenceNumber - prev.QuerySequenceNumber, 0.00001);
            }
        }

        [Test]
        public void ShouldStitchTwoFullMatchesWithAQueryAndTrackGap()
        {
            int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, trackOffset:  0, matchLength, trackLength: 20, queryLength: 10);
            var second = CreateEntry(queryOffset: 3, trackOffset: 13, 7, trackLength: 20, queryLength: 10);
            
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(first, second);
            
            Assert.AreEqual(20, concatenated.QueryLength);
            Assert.AreEqual(0, concatenated.QueryMatchStartsAt);
            Assert.AreEqual(0, concatenated.TrackMatchStartsAt); 
            Assert.IsFalse(concatenated.NoGaps);
            Assert.AreEqual(1, concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(10, 13, concatenated.Coverage.TrackGaps.First());
        }
        
        /**
         * first   -----
         * second       -----
         * third             -----
         * result  ---------------
         */
        [Test]
        public void ShouldStitchModeThanTwoConsecutiveQueries()
        {
            int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, trackOffset:  0, matchLength, queryLength: 10);
            var second = CreateEntry(queryOffset: 0, trackOffset: 10, matchLength, queryLength: 10); 
            var third = CreateEntry(queryOffset: 0, trackOffset: 20, matchLength, queryLength: 10);
            
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var concatenated = concatenator.Concat(first, second);
            var result = concatenator.Concat(concatenated, third);
            
            Assert.AreEqual(30, result.TrackCoverageWithPermittedGapsLength, 0.01);
            Assert.AreEqual(30, result.QueryLength, 0.01);
            Assert.AreEqual(0, result.QueryMatchStartsAt);
            Assert.AreEqual(0, result.TrackMatchStartsAt);
        }

        [Test]
        public void ShouldNotMergeAsOneIsContainedWithinTheOther()
        {
            int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, trackOffset: 0, matchLength, queryLength: 10, trackLength: 10);
            var second = CreateEntry(queryOffset: 0, trackOffset: 3, matchLength: 3, queryLength: 3, trackLength: 10);
            
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var result = concatenator.Concat(first, second);
            
            Assert.AreEqual(10, result.QueryLength, 0.01);
            Assert.AreEqual(10, result.TrackCoverageWithPermittedGapsLength, 0.01);
        }

        [Test]
        public void ShouldMergeMatchesThatOverlap()
        {
            int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, trackOffset: 0, matchLength, queryLength: 10, trackLength: 30); 
            var second  = CreateEntry(queryOffset: 0, trackOffset: 5, matchLength, queryLength: 10, trackLength: 30);
            
            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), false);
            
            var result = concatenator.Concat(first, second);
            
            Assert.AreEqual(0.5, result.TrackRelativeCoverage, 0.01);
            Assert.AreEqual(15, result.TrackCoverageWithPermittedGapsLength, 0.01);
            
            // it is debatable whether this is correct since it we have a query overlap of 5 seconds should we consider it or not?
            Assert.AreEqual(0.75, result.QueryRelativeCoverage, 0.01);
            Assert.AreEqual(20, result.QueryLength, 0.01);
        }

        [Test]
        public void ShouldBeAbleToConcatenateFromBothSides()
        {
            var left  = CreateEntry(queryOffset: 110, trackOffset:  0, matchLength: 10);
            var right = CreateEntry(queryOffset:   0, trackOffset: 10, matchLength: 20);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), autoSkipDetection: true);
            
            var a = concatenator.Concat(left, right);
            var b = concatenator.Concat(right, left);
            
            Assert.AreEqual(a.TrackRelativeCoverage, b.TrackRelativeCoverage, 0.0001);
        }

        [Test]
        public void ShouldDetectAGap()
        {
            var first  = CreateEntry(queryOffset: 0, trackOffset: 10, matchLength: 5, trackLength: 210, queryLength: 5); 
            var second  = CreateEntry(queryOffset: 0, trackOffset: 110, matchLength: 5, trackLength: 210, queryLength: 5);

            var concatenator = new ResultEntryConcatenator(new NullLoggerFactory(), autoSkipDetection: true);
            
            var result = concatenator.Concat(first, second);

            var trackGaps = result.Coverage.TrackGaps.ToArray();
            Assert.IsTrue(trackGaps.Any());
            Assert.AreEqual(3, trackGaps.Length);
            AssertDiscontinuity(0, 10, trackGaps[0]);
            AssertDiscontinuity(15, 110, trackGaps[1]);
            AssertDiscontinuity(115, 210, trackGaps[2]);
            Assert.AreEqual(10, result.Coverage.QueryLength, 0.01);
            Assert.AreEqual(10, result.Coverage.QueryCoverageWithPermittedGapsLength, 0.01);
            Assert.AreEqual(10, result.Coverage.QueryDiscreteCoverageLength, 0.01);
            Assert.AreEqual(10, result.Coverage.TrackCoverageWithPermittedGapsLength, 0.01);
            Assert.AreEqual(105, result.Coverage.TrackDiscreteCoverageLength, 0.01);
        }

        private ResultEntry CreateEntry(float queryOffset, float trackOffset, float matchLength, float trackLength = 30, float queryLength = 120, string trackId = "id")
        {
            var config = new DefaultQueryConfiguration
            {
                FingerprintConfiguration = new DefaultFingerprintConfiguration
                {
                    SampleRate = 8192 * 10
                },
                PermittedGap = 2.9
            };
            
            float fingerprintLength = (float)config.FingerprintConfiguration.FingerprintLengthInSeconds;
            var count = (int)Math.Round(matchLength / fingerprintLength);
            var bestPath = Enumerable.Range(0, count)
                .Select(i => (uint)i)
                .Select(seqNum => new MatchedWith(
                    querySequenceNumber: seqNum + (uint)Math.Round(queryOffset / fingerprintLength),
                    queryMatchAt:        seqNum * fingerprintLength + queryOffset,
                    trackSequenceNumber: seqNum + (uint)Math.Round(trackOffset / fingerprintLength),
                    trackMatchAt:        seqNum * fingerprintLength + trackOffset,
                    score: 100));
            return new ResultEntry(
                new TrackData(trackId, "artist", "title", trackLength, new ModelReference<uint>(1)),
                score: 100 * 100,
                matchedAt: DateTime.Now,
                new Coverage(bestPath, queryLength, trackLength, fingerprintLength, config.PermittedGap));
        }

        private static void AssertDiscontinuity(float start, float end, Gap discontinuity)
        {
            Assert.AreEqual(start, discontinuity.Start, Delta);
            Assert.AreEqual(end, discontinuity.End, Delta);
            Assert.AreEqual(end - start, discontinuity.LengthInSeconds, Delta);
        }
        
        private static double TrackDiscreteCoverage(Coverage coverage)
        {
            if (coverage == null)
            {
                return 0d;
            }
            
            return coverage.TrackDiscreteCoverageLength / coverage.TrackLength;
        }
    }
}