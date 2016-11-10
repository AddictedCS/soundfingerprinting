namespace SoundFingerprinting.Tests.Unit.Math
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Math;

    [TestClass]
    public class FScoreTest
    {
        [TestMethod]
        public void ShouldCalculateFScore()
        {
            var score = new FScore(90, 90, 10, 10);

            Assert.AreEqual(0.9d, score.Precision, 0.001);
            Assert.AreEqual(0.9d, score.Recall, 0.001);
            Assert.AreEqual(0.9d, score.F1, 0.001);
        }
    }
}
