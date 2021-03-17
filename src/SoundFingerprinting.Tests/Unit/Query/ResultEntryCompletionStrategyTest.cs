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
            strategy = new ResultEntryCompletionStrategy(PermittedGap);
        }

        [Test]
        public void ConstructorThrowsOnInvalidArgs()
        {
            Assert.Throws<ArgumentException>(() => new ResultEntryCompletionStrategy(double.NaN));
        }

        [Test]
        public void MissingResultEntryCannotContinue()
        {
            Assert.IsFalse(strategy.CanContinueInNextQuery(null));
        }

        [Test]
        public void MatchEndsMoreThanPermittedGapBeforeTheQueryEnd()
        {
            var entry = CreateResultEntry(gapAtTheEnd: PermittedGap + 0.1);

            Assert.IsFalse(strategy.CanContinueInNextQuery(entry));
        }

        [Test]
        public void MatchEndsExactlyPermittedGapBeforeTheQueryEnd()
        {
            var entry = CreateResultEntry(gapAtTheEnd: PermittedGap);

            Assert.IsTrue(strategy.CanContinueInNextQuery(entry));
        }

        private static ResultEntry CreateResultEntry(double gapAtTheEnd)
        {
            // query: [0 1 2 3 4 5 6 7 8 9]
            //           [match w gap][gap]
            const double queryLength = 10;
            const float queryMatchStartsAt = 1;
            const float trackMatchStartsAt = 0;
            const double score = 1;
            var matchedAt = DateTime.Now;
            var discreteCoverageLength = queryLength - queryMatchStartsAt - gapAtTheEnd;

            double fingerprintLength = 0.1;
            var matchedWith = Enumerable
                .Range(0, (int) (discreteCoverageLength / fingerprintLength))
                .Select(index => new MatchedWith(
                    (uint) (index + queryMatchStartsAt / fingerprintLength),   // query sequence at
                    queryMatchStartsAt + (float) (index * fingerprintLength),  // query matched at
                    (uint) (index + trackMatchStartsAt / fingerprintLength),   // track sequence at
                    trackMatchStartsAt + (float) (index * fingerprintLength),  // track matched at
                    100))
                .Take((int) (discreteCoverageLength / fingerprintLength))
                .ToList();

            var coverage = new Coverage(matchedWith, queryLength, 10, fingerprintLength, PermittedGap);
            var trackData = new TrackData("id", "artist", "title", 10, new ModelReference<uint>(1));
            var entry = new ResultEntry(trackData, score, matchedAt, coverage);

            Assert.AreEqual(entry.QueryMatchStartsAt + entry.DiscreteTrackCoverageLength, entry.QueryLength - gapAtTheEnd, Delta);
            return entry;
        }
    }
}