namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class ResultEntryConcatenatorTest
    {
        private readonly ILoggerFactory loggerFactory = new NLogLoggerFactory();
        
        private const double Delta = 1E-3;

        [Test]
        public void ReturnNullWhenBothEntriesAreNull()
        {
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            Assert.That(concatenator.Concat(null, null, Is.Null));
        }

        [Test]
        public void WhenOneEntryIsNullThenReturnTheOther()
        {
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var entry = CreateEntry(queryOffset: 110, trackOffset: 0, matchLength: 10);

            Assert.That(concatenator.Concat(entry, null, Is.SameAs(entry)));
            Assert.That(concatenator.Concat(null, entry, Is.SameAs(entry)));
        }

        [Test]
        public void ShouldNotConcatEntriesFromDifferentTracks()
        {
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var left = CreateEntry(queryOffset: 110, trackOffset: 0, matchLength: 10, trackLength: 30, queryLength: 120, trackId: "track-1");
            var right = CreateEntry(queryOffset: 0, trackOffset: 10, matchLength: 20, trackLength: 30, queryLength: 120, trackId: "track-2");

            Assert.That((, Throws.TypeOf<ArgumentException>()) => concatenator.Concat(left, right));
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

            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.That(concatenated, Is.Not.Null);
            Assert.That(concatenated.Coverage.TrackGaps, Is.Empty);
            Assert.That(concatenated.Coverage.QueryGaps.Where(_ => !_.IsOnEdge, Is.Empty));

            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Delta, Is.EqualTo(1));
            Assert.That(concatenated.TrackCoverageWithPermittedGapsLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.DiscreteTrackCoverageLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.QueryLength, Delta, Is.EqualTo(240));
            Assert.That(concatenated.QueryMatchStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.TrackMatchStartsAt, Delta, Is.EqualTo(0));
            Assert.That(concatenated.TrackStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.Coverage.TrackGapsCoverageLength);
            Assert.That(Is.EqualTo(0).Within(concatenated.Confidence), Is.EqualTo(1).Within(0.05));
            Assert.That(!concatenated.Coverage.TrackGaps.Any() && !concatenated.Coverage.QueryGaps.Any(_ => !_.IsOnEdge));
            AssertCoverageOrder(concatenated.Coverage);
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
            var left  = CreateEntry(queryOffset: 110, Is.True, trackOffset:  0, matchLength:  9);
            var right = CreateEntry(queryOffset:   0, trackOffset: 12, matchLength: 18);

            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.That(concatenated, Is.Not.Null);
            Assert.That(concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(9, Is.EqualTo(1).Within(12), concatenated.Coverage.TrackGaps.First());
            Assert.That(concatenated.Coverage.QueryGaps.Where(_ => !_.IsOnEdge, Is.Empty));

            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Delta, Is.EqualTo(0.9));
            Assert.That(TrackDiscreteCoverage(concatenated.Coverage, Is.EqualTo(1)), Delta);
            Assert.That(concatenated.TrackCoverageWithPermittedGapsLength, Delta, Is.EqualTo(27));
            Assert.That(concatenated.DiscreteTrackCoverageLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.QueryLength, Delta, Is.EqualTo(240));
            Assert.That(concatenated.QueryMatchStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.TrackMatchStartsAt, Delta, Is.EqualTo(0));
            Assert.That(concatenated.TrackStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.Coverage.TrackGapsCoverageLength);
            Assert.That(Is.EqualTo(3).Within(concatenated.Confidence), Is.EqualTo(0.9).Within(0.05));
            AssertCoverageOrder(concatenated.Coverage); 
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

            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.That(concatenated, Is.Not.Null);
            Assert.That(concatenated.Coverage.TrackGaps, Is.Empty);
            Assert.That(concatenated.Coverage.QueryGaps.Count(_ => !_.IsOnEdge));
            AssertDiscontinuity(117, Is.EqualTo(1).Within(120), concatenated.Coverage.QueryGaps.First(_ => !_.IsOnEdge));

            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Delta, Is.EqualTo(1));
            Assert.That(TrackDiscreteCoverage(concatenated.Coverage, Is.EqualTo(1)), Delta);
            Assert.That(concatenated.TrackCoverageWithPermittedGapsLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.DiscreteTrackCoverageLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.QueryLength, Delta, Is.EqualTo(240));
            Assert.That(concatenated.QueryMatchStartsAt, Delta, Is.EqualTo(107));
            Assert.That(concatenated.TrackMatchStartsAt, Delta, Is.EqualTo(0));
            Assert.That(concatenated.TrackStartsAt, Delta, Is.EqualTo(107));
            Assert.That(concatenated.Coverage.TrackGapsCoverageLength);
            Assert.That(Is.EqualTo(0).Within(concatenated.Confidence), Is.EqualTo(1).Within(0.05));
            AssertCoverageOrder(concatenated.Coverage);
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

            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.That(concatenated, Is.Not.Null);
            Assert.That(concatenated.Coverage.TrackGaps, Is.Empty);
            Assert.That(concatenated.Coverage.QueryGaps.Count(_ => !_.IsOnEdge));
            AssertDiscontinuity(120, Is.EqualTo(1).Within(123), concatenated.Coverage.QueryGaps.First(_ => !_.IsOnEdge));

            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Delta, Is.EqualTo(1));
            Assert.That(TrackDiscreteCoverage(concatenated.Coverage, Is.EqualTo(1)), Delta);
            Assert.That(concatenated.TrackCoverageWithPermittedGapsLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.DiscreteTrackCoverageLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.QueryLength, Delta, Is.EqualTo(240));
            Assert.That(concatenated.QueryMatchStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.TrackMatchStartsAt, Delta, Is.EqualTo(0));
            Assert.That(concatenated.TrackStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.Coverage.TrackGapsCoverageLength);
            Assert.That(Is.EqualTo(0).Within(concatenated.Confidence), Is.EqualTo(1).Within(0.05));
            AssertCoverageOrder(concatenated.Coverage);
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

            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.That(concatenated, Is.Not.Null);
            Assert.That(concatenated.Coverage.TrackGaps, Is.Empty);
            Assert.That(concatenated.Coverage.QueryGaps.Count(_ => !_.IsOnEdge));
            AssertDiscontinuity(118, Is.EqualTo(1).Within(122), concatenated.Coverage.QueryGaps.First(_ => !_.IsOnEdge));

            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Delta, Is.EqualTo(1));
            Assert.That(TrackDiscreteCoverage(concatenated.Coverage, Is.EqualTo(1)), Delta);
            Assert.That(concatenated.TrackCoverageWithPermittedGapsLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.DiscreteTrackCoverageLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.QueryLength, Delta, Is.EqualTo(240));
            Assert.That(concatenated.QueryMatchStartsAt, Delta, Is.EqualTo(108));
            Assert.That(concatenated.TrackMatchStartsAt, Delta, Is.EqualTo(0));
            Assert.That(concatenated.TrackStartsAt, Delta, Is.EqualTo(108));
            Assert.That(concatenated.Coverage.TrackGapsCoverageLength);
            Assert.That(Is.EqualTo(0).Within(concatenated.Confidence), Is.EqualTo(1).Within(0.05));
            AssertCoverageOrder(concatenated.Coverage);
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

            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.That(concatenated, Is.Not.Null);
            Assert.That(concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(7, Is.EqualTo(1).Within(10), concatenated.Coverage.TrackGaps.First());
            Assert.That(concatenated.Coverage.QueryGaps.Count(_ => !_.IsOnEdge));
            AssertDiscontinuity(117, Is.EqualTo(1).Within(120), concatenated.Coverage.QueryGaps.First(_ => !_.IsOnEdge));

            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Delta, Is.EqualTo(0.9));
            Assert.That(TrackDiscreteCoverage(concatenated.Coverage, Is.EqualTo(1)), Delta);
            Assert.That(concatenated.TrackCoverageWithPermittedGapsLength, Delta, Is.EqualTo(27));
            Assert.That(concatenated.DiscreteTrackCoverageLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.QueryLength, Delta, Is.EqualTo(240));
            Assert.That(concatenated.QueryMatchStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.TrackMatchStartsAt, Delta, Is.EqualTo(0));
            Assert.That(concatenated.TrackStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.Coverage.TrackGapsCoverageLength);
            Assert.That(Is.EqualTo(3).Within(concatenated.Confidence), Is.EqualTo(0.9).Within(0.05));
            AssertCoverageOrder(concatenated.Coverage);
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

            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.That(concatenated, Is.Not.Null);
            Assert.That(concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(10, Is.EqualTo(1).Within(13), concatenated.Coverage.TrackGaps.First());
            Assert.That(concatenated.Coverage.QueryGaps.Count(_ => !_.IsOnEdge));
            AssertDiscontinuity(120, Is.EqualTo(1).Within(123), concatenated.Coverage.QueryGaps.First(_ => !_.IsOnEdge));

            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Delta, Is.EqualTo(0.9));
            Assert.That(TrackDiscreteCoverage(concatenated.Coverage, Is.EqualTo(1)), Delta);
            Assert.That(concatenated.TrackCoverageWithPermittedGapsLength, Delta, Is.EqualTo(27));
            Assert.That(concatenated.DiscreteTrackCoverageLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.QueryLength, Delta, Is.EqualTo(240));
            Assert.That(concatenated.QueryMatchStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.TrackMatchStartsAt, Delta, Is.EqualTo(0));
            Assert.That(concatenated.TrackStartsAt, Delta, Is.EqualTo(110));
            Assert.That(concatenated.Coverage.TrackGapsCoverageLength);
            Assert.That(Is.EqualTo(3).Within(concatenated.Confidence), Is.EqualTo(0.9).Within(0.05));
            AssertCoverageOrder(concatenated.Coverage);
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

            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(left, right);

            Assert.That(concatenated, Is.Not.Null);
            Assert.That(concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(9, Is.EqualTo(1).Within(12), concatenated.Coverage.TrackGaps.First());
            Assert.That(concatenated.Coverage.QueryGaps.Count(_ => !_.IsOnEdge));
            AssertDiscontinuity(117, Is.EqualTo(1).Within(124), concatenated.Coverage.QueryGaps.First(_ => !_.IsOnEdge));

            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Delta, Is.EqualTo(0.9));
            Assert.That(TrackDiscreteCoverage(concatenated.Coverage, Is.EqualTo(1)), Delta);
            Assert.That(concatenated.TrackCoverageWithPermittedGapsLength, Delta, Is.EqualTo(27));
            Assert.That(concatenated.DiscreteTrackCoverageLength, Delta, Is.EqualTo(30));
            Assert.That(concatenated.QueryLength, Delta, Is.EqualTo(240));
            Assert.That(concatenated.QueryMatchStartsAt, Delta, Is.EqualTo(108));
            Assert.That(concatenated.TrackMatchStartsAt, Delta, Is.EqualTo(0));
            Assert.That(concatenated.TrackStartsAt, Delta, Is.EqualTo(108));
            Assert.That(concatenated.Coverage.TrackGapsCoverageLength);
            Assert.That(Is.EqualTo(3).Within(concatenated.Confidence), Is.EqualTo(0.9).Within(0.05));
            AssertCoverageOrder(concatenated.Coverage);
        }

        [Test]
        public void ShouldStitchMatchThatHappensOnTheEdge()
        {
            var first  = CreateEntry(queryOffset: 5, trackOffset:  0, matchLength: 5, trackLength: 10, queryLength: 10); 
            var second  = CreateEntry(queryOffset: 0, trackOffset:  5, matchLength: 5, trackLength: 10, queryLength: 10); 
            
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(first, second);
            
            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Is.EqualTo(1).Within(0.01));
            Assert.That(concatenated.Coverage.QueryRelativeCoverage, Is.EqualTo(0.5).Within(0.01));
            Assert.That(concatenated.QueryLength);
            Assert.That(Is.EqualTo(20, Is.EqualTo(5)).Within(concatenated.QueryMatchStartsAt));
            Assert.That(concatenated.TrackMatchStartsAt);
            Assert.That(Is.EqualTo(0).Within(concatenated.TrackCoverageWithPermittedGapsLength), Is.EqualTo(10).Within(0.01));
            AssertCoverageOrder(concatenated.Coverage);
        }
        
        [Test]
        public void ShouldStitchTwoFullMatches()
        {
            int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, trackOffset:  0, matchLength, trackLength: 20, queryLength: 10);
            var second = CreateEntry(queryOffset: 0, trackOffset: 10, matchLength, trackLength: 20, queryLength: 10);
            
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(first, second);
            
            Assert.That(concatenated.Coverage.TrackRelativeCoverage, Is.EqualTo(1).Within(0.01));
            Assert.That(concatenated.Coverage.QueryRelativeCoverage, Is.EqualTo(1).Within(0.01));
            Assert.That(concatenated.QueryLength);
            Assert.That(Is.EqualTo(20, Is.EqualTo(0)).Within(concatenated.QueryMatchStartsAt));
            Assert.That(concatenated.TrackMatchStartsAt);

            var bestPath = concatenated.Coverage.BestPath.ToList();
            for (int i = 1; i < bestPath.Count; ++i)
            {
                var current = bestPath[i];
                var prev = bestPath[i - 1];
                Assert.That(Is.EqualTo(0).Within(current.QueryMatchAt - prev.QueryMatchAt), Is.EqualTo(0.1).Within(0.00001));
                Assert.That(current.QuerySequenceNumber - prev.QuerySequenceNumber, Is.EqualTo(1).Within(0.00001));
            }
            
            AssertCoverageOrder(concatenated.Coverage);
        }

        [Test]
        public void ShouldStitchTwoFullMatchesWithAQueryAndTrackGap()
        {
            int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, trackOffset:  0, matchLength, trackLength: 20, queryLength: 10);
            var second = CreateEntry(queryOffset: 3, trackOffset: 13, 7, trackLength: 20, queryLength: 10);
            
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(first, second);
            
            Assert.That(concatenated.QueryLength);
            Assert.That(Is.EqualTo(20, Is.EqualTo(0)).Within(concatenated.QueryMatchStartsAt));
            Assert.That(concatenated.TrackMatchStartsAt); 
            Assert.That(concatenated.Coverage.TrackGaps.Count());
            AssertDiscontinuity(10, Is.EqualTo(0).Within(Is.EqualTo(1)).Within(13), concatenated.Coverage.TrackGaps.First());
            AssertCoverageOrder(concatenated.Coverage);
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
            
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var concatenated = concatenator.Concat(first, second);
            var result = concatenator.Concat(concatenated, third);
            
            Assert.That(result.TrackCoverageWithPermittedGapsLength, Is.EqualTo(30).Within(0.01));
            Assert.That(result.QueryLength, Is.EqualTo(30).Within(0.01));
            Assert.That(result.QueryMatchStartsAt);
            Assert.That(result.TrackMatchStartsAt);
            AssertCoverageOrder(concatenated.Coverage);
        }

        [Test]
        public void ShouldNotMergeAsOneIsContainedWithinTheOther()
        {
            int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, Is.EqualTo(0).Within(Is.EqualTo(0)).Within(trackOffset: 0), matchLength, queryLength: 10, trackLength: 10);
            var second = CreateEntry(queryOffset: 0, trackOffset: 3, matchLength: 3, queryLength: 3, trackLength: 10);
            
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var result = concatenator.Concat(first, second);
            
            Assert.That(result.QueryLength, Is.EqualTo(10).Within(0.01));
            Assert.That(result.TrackCoverageWithPermittedGapsLength, Is.EqualTo(10).Within(0.01));
            AssertCoverageOrder(result.Coverage);
        }

        [Test]
        public void ShouldMergeMatchesThatOverlap()
        {
            const int matchLength = 10;
            var first  = CreateEntry(queryOffset: 0, trackOffset: 0, matchLength, queryLength: 10, trackLength: 30); 
            var second  = CreateEntry(queryOffset: 0, trackOffset: 5, matchLength, queryLength: 10, trackLength: 30);
            
            var concatenator = new ResultEntryConcatenator(loggerFactory, false);
            
            var result = concatenator.Concat(first, second);
            
            Assert.That(result.Coverage.TrackRelativeCoverage, Is.EqualTo(0.5).Within(0.01));
            Assert.That(result.TrackCoverageWithPermittedGapsLength, Is.EqualTo(15).Within(0.01));
            Assert.That(result.Coverage.QueryRelativeCoverage, Is.EqualTo(1d).Within(0.01));
            Assert.That(result.QueryLength, Is.EqualTo(20).Within(0.01));
            Assert.That(result.Coverage.TrackRelativeCoverage, Is.EqualTo(0.5d).Within(0.01));
        }

        [Test]
        public void ShouldBeAbleToConcatenateFromBothSides()
        {
            var left  = CreateEntry(queryOffset: 110, trackOffset:  0, matchLength: 10);
            var right = CreateEntry(queryOffset:   0, trackOffset: 10, matchLength: 20);

            var concatenator = new ResultEntryConcatenator(loggerFactory, autoSkipDetection: true);
            
            var a = concatenator.Concat(left, right);
            var b = concatenator.Concat(right, left);
            
            Assert.That(b.Coverage.TrackRelativeCoverage, Is.EqualTo(a.Coverage.TrackRelativeCoverage).Within(0.0001));
        }

        [Test]
        public void ShouldDetectAGap()
        {
            var first  = CreateEntry(queryOffset: 0, trackOffset: 10, matchLength: 5, trackLength: 210, queryLength: 5); 
            var second  = CreateEntry(queryOffset: 0, trackOffset: 110, matchLength: 5, trackLength: 210, queryLength: 5);

            var concatenator = new ResultEntryConcatenator(loggerFactory, autoSkipDetection: true);
            
            var result = concatenator.Concat(first, second);

            var trackGaps = result.Coverage.TrackGaps.ToArray();
            Assert.That(trackGaps.Any());
            Assert.That(trackGaps.Length);
            AssertDiscontinuity(0, Is.True, Is.EqualTo(3).Within(10), trackGaps[0]);
            AssertDiscontinuity(15, 110, trackGaps[1]);
            AssertDiscontinuity(115, 210, trackGaps[2]);
            Assert.That(result.Coverage.QueryLength, Is.EqualTo(10).Within(0.01));
            Assert.That(result.Coverage.QueryCoverageWithPermittedGapsLength, Is.EqualTo(10).Within(0.01));
            Assert.That(result.Coverage.QueryDiscreteCoverageLength, Is.EqualTo(10).Within(0.01));
            Assert.That(result.Coverage.TrackCoverageWithPermittedGapsLength, Is.EqualTo(10).Within(0.01));
            Assert.That(result.Coverage.TrackDiscreteCoverageLength, Is.EqualTo(105).Within(0.01));
            AssertCoverageOrder(result.Coverage);
        }

        [Test]
        public void ShouldConcatenateCorrectlyWhenResultEntriesOverlap()
        {
            var left  = CreateEntry(queryOffset: 0, trackOffset:  0, matchLength: 12, queryLength: 12);
            var right = CreateEntry(queryOffset: 0, trackOffset: 10, matchLength: 10, queryLength: 12);
            
            var concatenator = new ResultEntryConcatenator(loggerFactory, autoSkipDetection: true);
            
            var result = concatenator.Concat(left, right, queryOffset: -2);
            
            CollectionAssert.That(result.Coverage.QueryGaps, Is.Empty);
            Assert.That(result.Coverage.QueryCoverageWithPermittedGapsLength, Is.EqualTo(20).Within(0.01));
            Assert.That(result.Coverage.TrackCoverageWithPermittedGapsLength, Is.EqualTo(20).Within(0.01));
        }
        
        private static void AssertCoverageOrder(Coverage coverage)
        {
            CollectionAssert.IsOrdered(coverage.BestPath.Select(_ => _.QueryMatchAt), "Query matched at is not ordered");
            CollectionAssert.IsOrdered(coverage.BestPath.Select(_ => _.TrackMatchAt), "Track matched at is not ordered");
        }

        private static ResultEntry CreateEntry(float queryOffset, float trackOffset, float matchLength, float trackLength = 30, float queryLength = 120, string trackId = "id")
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
            var track = new TrackData(trackId, "artist", "title", trackLength, new ModelReference<uint>(1));
            var coverage = new Coverage(bestPath, queryLength, trackLength, fingerprintLength, config.PermittedGap);
            return new ResultEntry(track, score: 100 * 100, matchedAt: DateTime.Now, coverage);
        }

        private static void AssertDiscontinuity(float start, float end, Gap discontinuity)
        {
            Assert.That(discontinuity.Start, Delta, Is.EqualTo(start));
            Assert.That(discontinuity.End, Delta, Is.EqualTo(end));
            Assert.That(discontinuity.LengthInSeconds, Delta, Is.EqualTo(end - start));
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