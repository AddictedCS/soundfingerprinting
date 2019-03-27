namespace SoundFingerprinting.Tests.Unit.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using ProtoBuf;
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
        public void ShouldMergeCorrectly()
        {
            float acc = 8192 / 5512f;
            var a = TimedHashes.Empty;
            var dateTime = DateTime.Now;
            var b = new TimedHashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {1}, 0, 0, new string[0])}), dateTime);
            var c = new TimedHashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {2}, 0, 0f, new string[0])}), dateTime.AddSeconds(acc));
            var d = new TimedHashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {3}, 0, 0f, new string[0])}), dateTime.AddSeconds(2 * acc));

            Assert.IsTrue(a.MergeWith(b, out var x));
            Assert.IsTrue(x.MergeWith(c, out var y));
            Assert.IsTrue(y.MergeWith(d, out var z));

            Assert.AreEqual(dateTime, z.StartsAt);
            Assert.AreEqual(3, z.HashedFingerprints.Count);
            Assert.AreEqual(3 * acc, z.TotalSeconds, 0.001);
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

        [Test]
        public void CanSerializeAndDeserialize()
        {
            var list = GetHashedFingerprints();

            var timed = new TimedHashes(list, DateTime.Now);
            var buffer = Serialize(timed);
            var deserialized = Deserialize(buffer);
            Assert.AreEqual(timed.HashedFingerprints.Count, deserialized.HashedFingerprints.Count);
            Assert.AreEqual(timed.StartsAt, deserialized.StartsAt);

            for (int i = 0; i < timed.HashedFingerprints.Count; ++i)
            {
                HashedFingerprint a = timed.HashedFingerprints[i];
                HashedFingerprint b = deserialized.HashedFingerprints[i];
                
                Assert.AreEqual(a.StartsAt, b.StartsAt);
                Assert.AreEqual(a.SequenceNumber, b.SequenceNumber);
                CollectionAssert.AreEqual(a.HashBins, b.HashBins);
            }
        }

        private static TimedHashes Deserialize(byte[] buffer)
        {
            TimedHashes deserialized;
            using (var stream = new MemoryStream(buffer))
            {
                deserialized = Serializer.DeserializeWithLengthPrefix<TimedHashes>(stream, PrefixStyle.Fixed32);
            }

            return deserialized;
        }

        private static byte[] Serialize(TimedHashes timed)
        {
            byte[] buffer;
            using (var stream = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(stream, timed, PrefixStyle.Fixed32);
                stream.Flush();
                buffer = stream.GetBuffer();
            }

            return buffer;
        }

        private static List<HashedFingerprint> GetHashedFingerprints()
        {
            var random = new Random();
            var list = new List<HashedFingerprint>();
            for (int i = 0; i < 100; ++i)
            {
                int[] hashes = new int[25];
                for (int j = 0; j < 25; ++j)
                {
                    int hash = random.Next();
                    hashes[j] = hash;
                }

                list.Add(new HashedFingerprint(hashes, (uint) (i + 1), i * 1.48f, Enumerable.Empty<string>()));
            }

            return list;
        }
    }
}