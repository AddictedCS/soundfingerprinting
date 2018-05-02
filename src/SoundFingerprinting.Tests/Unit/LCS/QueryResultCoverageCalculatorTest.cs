namespace SoundFingerprinting.Tests.Unit.LCS
{
    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.LCS;

    [TestFixture]
    public class QueryResultCoverageCalculatorTest
    {
        private readonly QueryResultCoverageCalculator qrc = new QueryResultCoverageCalculator(new LongestIncreasingTrackSequence());

        [Test]
        public void ShouldIdentifyLongestMatch()
        {
            var matches = TestUtilities.GetMatchedWith(new float[] { 5, 9, 11, 14 }, new float[] { 0, 5, 9, 10 });

            var coverage = qrc.GetCoverage(matches, 9d, new DefaultFingerprintConfiguration());

            Assert.AreEqual(5.4586, coverage.SourceMatchLength, 0.001);
        }

        [Test]
        public void ShouldSelectBestLongestMatch()
        {
            var matches = TestUtilities.GetMatchedWith(new float[] { 1, 2, 3, 4, 5 }, new float[] { 1, 2, 9, 11, 12 });

            var coverage = qrc.GetCoverage(matches, 5d, new DefaultFingerprintConfiguration());

            Assert.AreEqual(3.9724, coverage.SourceMatchLength, 0.01);
            Assert.AreEqual(12.48, coverage.SourceCoverageLength, 0.01);
            Assert.AreEqual(-6, coverage.TrackStartsAt);
        }
    }
}
