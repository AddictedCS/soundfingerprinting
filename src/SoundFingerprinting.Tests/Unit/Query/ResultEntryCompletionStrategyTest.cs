namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class ResultEntryCompletionStrategyTest
    {
        private const double PermittedGap = 3;
        private const double Delta = 1E-3;

        private ICompletionStrategy<ResultEntry> strategy;

        [SetUp]
        public void SetUp()
        {
            strategy = new ResultEntryCompletionStrategy(0d);
        }

        [Test]
        public void MissingResultEntryCannotContinue()
        {
			Assert.That(strategy.CanContinueInNextQuery(null), Is.False);
        }

        [TestCase(120, 60, 10, 0, false)]
        [TestCase(120, 115, 10, 0, true)]
        [TestCase(2, 0, 10, 5, true)]
        [TestCase(5, 0, 10, 5, false)]
        [TestCase(120, 100, 30, 10, false)]
        [TestCase(10, 0, 30, 0, true)]
        [TestCase(10, 0, 25, 25, false)]
        public void ShouldCoverAllScenarios(double queryLength, float queryMatchStartsAt, double trackLength, float trackMatchStartsAt, bool expected)
        {
            var entry = CreateResultEntry(0, queryLength, trackLength, queryMatchStartsAt, trackMatchStartsAt);
			Assert.That(strategy.CanContinueInNextQuery(entry), Is.EqualTo(expected)); 
        }

        [TestCase(39, 28, 120, true)]
        [TestCase(140, 129, 1710, false)]
        [TestCase(111, 100, 1710, true)]
        [TestCase(150, 100, 200, true)]
        [TestCase(700, 190, 1710, false)]
        public void ShouldCompleteEntriesStalePastTheBridgeableCeilingInsteadOfWaitingForRemainingTrackLength(double queryLength, double gapAtTheEnd, double trackLength, bool expected)
        {
            // with a capped ceiling an entry that stopped extending may bridge a non-matching stretch of at most the
            // ceiling: an 11s match on a 2-minute track survives a 28s dropout exactly as before, while a sporadic
            // ~11s brush against a much longer track completes shortly after it stops extending instead of being
            // held for the track's remaining length
            var cappedStrategy = new ResultEntryCompletionStrategy(0d, maxStalenessSeconds: 120d);
            var entry = CreateResultEntry(gapAtTheEnd, queryLength, trackLength);
            Assert.That(cappedStrategy.CanContinueInNextQuery(entry), Is.EqualTo(expected));
        }

        [TestCase(140, 129, 1710)]
        [TestCase(700, 190, 1710)]
        [TestCase(1500, 1400, 1710)]
        public void ShouldPreserveHistoricalBehaviorByDefaultRegardlessOfStaleness(double queryLength, double gapAtTheEnd, double trackLength)
        {
            // the default ceiling is double.MaxValue so library consumers observe no behavior change across upgrades:
            // stale entries on long tracks keep waiting for the remaining track length exactly as before
            var entry = CreateResultEntry(gapAtTheEnd, queryLength, trackLength);
            Assert.That(strategy.CanContinueInNextQuery(entry), Is.True);
        }

        private static ResultEntry CreateResultEntry(double gapAtTheEnd, double queryLength = 10, double trackLength = 10, float queryMatchStartsAt = 0, float trackMatchStartsAt = 0)
        {
            // query: [0 1 2 3 4 5 6 7 8 9]
            //           [match w gap][gap]
            const double score = 1;
            var matchedAt = DateTime.Now;
            var discreteCoverageLength = queryLength - queryMatchStartsAt - gapAtTheEnd;

            double fingerprintLength = 0.1;
            var matchedWith = Enumerable
                .Range(0, (int)(discreteCoverageLength / fingerprintLength))
                .Select(index => new MatchedWith(
                    (uint)(index + queryMatchStartsAt / fingerprintLength), // query sequence at
                    queryMatchStartsAt + (float)(index * fingerprintLength), // query matched at
                    (uint)(index + trackMatchStartsAt / fingerprintLength), // track sequence at
                    trackMatchStartsAt + (float)(index * fingerprintLength), // track matched at
                    100))
                .Take((int)(discreteCoverageLength / fingerprintLength))
                .ToList();

            var coverage = new Coverage(matchedWith, queryLength, trackLength, fingerprintLength, PermittedGap, 0);
            var trackData = new TrackData("id", "artist", "title", trackLength, new ModelReference<uint>(1));
            var entry = new ResultEntry(trackData, score, matchedAt, coverage);

			Assert.That(entry.QueryLength - gapAtTheEnd, Is.EqualTo(entry.QueryMatchStartsAt + entry.DiscreteTrackCoverageLength).Within(Delta));
            return entry;
        }
    }
}