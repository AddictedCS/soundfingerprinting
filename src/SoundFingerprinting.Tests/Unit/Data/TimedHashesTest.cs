namespace SoundFingerprinting.Tests.Unit.Data
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using SoundFingerprinting.Data;

    [TestFixture]
    public class TimedHashesTest
    {
        [Test]
        public void ShouldMerge()
        {
            float one = 8192f / 5512;
            var a = new TimedHashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {1}, 1, 1.48f, new string[0]),
                    new HashedFingerprint(new[] {1}, 2, 2 * 1.48f, new string[0]),
                    new HashedFingerprint(new[] {1}, 0, 0, new string[0]),
                },
                DateTime.Parse("01/15/2019 10:00:00"));

            var b = new TimedHashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {2}, 1, 1.48f, new string[0]),
                    new HashedFingerprint(new[] {2}, 2, 2 * 1.48f, new string[0]),
                    new HashedFingerprint(new[] {2}, 0, 0, new string[0]),
                },
                DateTime.Parse("01/15/2019 10:00:01"));

            Assert.IsTrue(a.MergeWith(b, out var result));
            Assert.AreEqual(0, result.HashedFingerprints[0].StartsAt);
            Assert.AreEqual(1f, result.HashedFingerprints[1].StartsAt, 0.0001);
            Assert.AreEqual(one, result.HashedFingerprints[2].StartsAt, 0.0001);
            Assert.AreEqual(1 + one, result.HashedFingerprints[3].StartsAt, 0.0001);
            Assert.AreEqual(2 * one, result.HashedFingerprints[4].StartsAt, 0.0001);
            Assert.AreEqual(1 + 2 * one, result.HashedFingerprints[5].StartsAt, 0.0001);
        }
    }
}