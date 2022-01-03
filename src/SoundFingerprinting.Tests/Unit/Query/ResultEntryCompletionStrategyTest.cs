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
            Assert.IsFalse(strategy.CanContinueInNextQuery(null));
        }

        [TestCase(120, 60, 10, 0, false)]
        [TestCase(120, 115, 10, 0, true)]
        [TestCase(2, 0, 10, 5, true)]
        [TestCase(5, 0, 10, 5, false)]
        [TestCase(120, 100, 30, 10, false)]
        public void ShouldCoverAllScenarios(double queryLength, float queryMatchStartsAt, double trackLength, float trackMatchStartsAt, bool expected)
        {
            var entry = CreateResultEntry(0, queryLength, trackLength, queryMatchStartsAt, trackMatchStartsAt); 
            Assert.AreEqual(expected, strategy.CanContinueInNextQuery(entry)); 
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

            var coverage = new Coverage(matchedWith, queryLength, trackLength, fingerprintLength, PermittedGap);
            var trackData = new TrackData("id", "artist", "title", trackLength, new ModelReference<uint>(1));
            var entry = new ResultEntry(trackData, score, matchedAt, coverage);

            Assert.AreEqual(entry.QueryMatchStartsAt + entry.DiscreteTrackCoverageLength, entry.QueryLength - gapAtTheEnd, Delta);
            return entry;
        }
    }
}