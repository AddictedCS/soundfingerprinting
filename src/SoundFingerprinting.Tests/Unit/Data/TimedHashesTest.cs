namespace SoundFingerprinting.Tests.Unit.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Tests.Integration;

    [TestFixture]
    public class TimedHashesTest
    {
        [Test]
        public void ShouldMerge()
        {
            float one = 8192f / 5512;
            var a = new TimedHashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {1}, 1, one, new string[0]),
                    new HashedFingerprint(new[] {1}, 2, 2 * one, new string[0]),
                    new HashedFingerprint(new[] {1}, 0, 0, new string[0]),
                },
                DateTime.Parse("01/15/2019 10:00:00"));

            var b = new TimedHashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {2}, 1, one, new string[0]),
                    new HashedFingerprint(new[] {2}, 2, 2 * one, new string[0]),
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

        [Test]
        public void ShouldMergeLongSequences()
        {
            var first = new List<HashedFingerprint>();
            var second = new List<HashedFingerprint>();
            
            float one = 8192f / 5512;
            for (int i = 0; i < 100; ++i)
            {
                first.Add(new HashedFingerprint(new[] {1}, (uint) i, i * one, new[] {"USA"}));
                second.Add(new HashedFingerprint(new[] {2}, (uint) i, i * one, new[] {"Canada"}));
            }

            var r = new Random();
            var a = new TimedHashes(first.OrderBy(x => r.Next()).ToList(), DateTime.Parse("01/15/2019 10:00:00"));
            var b = new TimedHashes(second.OrderBy(x => r.Next()).ToList(), DateTime.Parse("01/15/2019 10:00:01.3"));

            Assert.IsTrue(a.MergeWith(b, out var c));
            for (int i = 0; i < 200; ++i)
            {
                Assert.AreEqual(i, c.HashedFingerprints[i].SequenceNumber);
                if (i % 2 == 0)
                {
                    Assert.AreEqual(one * (i / 2), c.HashedFingerprints[i].StartsAt, 0.0001);
                    CollectionAssert.AreEqual(new[] {1}, c.HashedFingerprints[i].HashBins);
                    CollectionAssert.AreEqual(new[] {"USA"}, c.HashedFingerprints[i].Clusters);
                }
                else
                {
                    Assert.AreEqual(1.3f + one * (i / 2), c.HashedFingerprints[i].StartsAt, 0.0001);
                    CollectionAssert.AreEqual(new[] {2}, c.HashedFingerprints[i].HashBins);
                    CollectionAssert.AreEqual(new[] {"Canada"}, c.HashedFingerprints[i].Clusters);
                }
            }
        }

        [Test]
        public void CantMergeSinceTheGapIsTooBig()
        {
            var a = new TimedHashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {1}, 0, 0, new string[0]),
                },
                DateTime.Parse("01/15/2019 10:00:00"));

            var b = new TimedHashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {2}, 0, 0, new string[0]),
                },
                DateTime.Parse("01/15/2019 10:01:00"));
            
            Assert.IsFalse(a.MergeWith(b, out _));
        }
    }
}