namespace SoundFingerprinting.Tests.Unit.Math
{
    using System.Collections.Concurrent;

    using NUnit.Framework;

    using SoundFingerprinting.Math;

    [TestFixture]
    public class HammingDistanceResultStatisticsTest
    {
        [Test]
        public void ShouldCalculatePercentilesCorrenctly()
        {
            var stats = HammingDistanceResultStatistics.From(
                new ConcurrentBag<int> { 5, 4, 3, 2, 1 },
                new ConcurrentBag<int>(),
                new ConcurrentBag<int>(),
                new[] { 0.9, 0.95, 0.98 });

            var percentiles = stats.TruePositivePercentile;

            Assert.AreEqual(4.6, percentiles[0], 0.001);
            Assert.AreEqual(4.8, percentiles[1], 0.001);
            Assert.AreEqual(4.92, percentiles[2], 0.001);
        }
    }
}
