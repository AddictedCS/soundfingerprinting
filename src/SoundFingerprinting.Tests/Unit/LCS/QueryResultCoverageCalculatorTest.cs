namespace SoundFingerprinting.Tests.Unit.LCS
{
    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.LCS;

    [TestFixture]
    public class QueryResultCoverageCalculatorTest
    {
        private readonly QueryResultCoverageCalculator qrc = new QueryResultCoverageCalculator(new LongestIncreasingTrackSequence());

        private readonly double fingerprintLengthInSeconds = new DefaultFingerprintConfiguration().FingerprintLengthInSeconds;

        [Test]
        public void ShouldIdentifyLongestMatch()
        {
            const double QueryLength = 9d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 5, 9, 11, 14 }, new float[] { 0, 5, 9, 10 });

            var coverage = qrc.GetCoverage(matches, QueryLength, fingerprintLengthInSeconds);

            Assert.AreEqual(5.4586, coverage.SourceMatchLength, 0.001);
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            const double QueryLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 1, 2, 3, 4, 5 }, new float[] { 1, 2, 9, 11, 12 });

            var coverage = qrc.GetCoverage(matches, QueryLength, fingerprintLengthInSeconds);

            Assert.AreEqual(3.9724, coverage.SourceMatchLength, 0.01);
            Assert.AreEqual(4.486, coverage.SourceCoverageLength, 0.01);
            Assert.AreEqual(-6, coverage.TrackStartsAt);
        }

        [Test]
        public void ShouldDisregardJingleSinceTheGapIsTooBig()
        {
            const double QueryLength = 5d;
            var matches = TestUtilities.GetMatchedWith(new float[] { 1, 4, 5, 1, 2 }, new float[] { 1, 3, 4, 10, 11 });

            var coverage = qrc.GetCoverage(matches, QueryLength, fingerprintLengthInSeconds);

            Assert.AreEqual(3.9724, coverage.SourceMatchLength, 0.01);
            Assert.AreEqual(4.486, coverage.SourceCoverageLength, 0.01);
        }
    }
}
