namespace SoundFingerprinting.Tests.Unit.Math
{
    using System.Collections.Concurrent;

    using NUnit.Framework;

    using SoundFingerprinting.Math;

    [TestFixture]
    public class HammingDistanceResultStatisticsTest
    {
        [Test]
        public void ShouldCalculatePercentilesCorrectly()
        {
            var stats = HammingDistanceResultStatistics.From(
                new ConcurrentBag<int> { 5, 4, 3, 2, 1 },
                new ConcurrentBag<int>(),
                new ConcurrentBag<int>(),
                new[] { 0.9, 0.95, 0.98 });

            var percentiles = stats.TruePositivePercentile;

            Assert.That(percentiles[0], Is.EqualTo(4.6).Within(0.001));
            Assert.That(percentiles[1], Is.EqualTo(4.8).Within(0.001));
            Assert.That(percentiles[2], Is.EqualTo(4.92).Within(0.001));
        }
    }
}
