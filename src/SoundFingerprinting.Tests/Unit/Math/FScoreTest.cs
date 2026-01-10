namespace SoundFingerprinting.Tests.Unit.Math
{
    using NUnit.Framework;

    using SoundFingerprinting.Math;

    [TestFixture]
    public class FScoreTest
    {
        [Test]
        public void ShouldCalculateFScore()
        {
            var score = new FScore(90, 90, 10, 10);

            Assert.That(score.Precision, Is.EqualTo(0.9d).Within(0.001));
            Assert.That(score.Recall, Is.EqualTo(0.9d).Within(0.001));
            Assert.That(score.F1, Is.EqualTo(0.9d).Within(0.001));
        }
    }
}
