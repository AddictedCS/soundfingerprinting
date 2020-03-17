namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class HashedFingerprintsExtensionsTest
    {
        [Test]
        public void ShouldCalculateQueryLengthCorrectly()
        {
            var config = new DefaultFingerprintConfiguration();
            float delta = 0.05f;
            int runs = 1000;
            var bag = new ConcurrentBag<HashedFingerprint>();
            Parallel.For(0, runs, i =>
            {
                var hashed = new HashedFingerprint(new int[0], (uint)i, i * delta);
                bag.Add(hashed);
            });


            double length = bag.QueryLength(config);

            Assert.AreEqual(length, delta * (runs - 1) + config.FingerprintLengthInSeconds, 0.0001);
        }
    }
}