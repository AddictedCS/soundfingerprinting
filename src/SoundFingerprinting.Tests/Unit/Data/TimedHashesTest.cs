namespace SoundFingerprinting.Tests.Unit.Data
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using ProtoBuf;
    using SoundFingerprinting.Data;

    [TestFixture]
    public class TimedHashesTest
    {
        private const double FingerprintCount = 8192.0d / 5512;

        [Test]
        public void ShouldMerge()
        {
            var dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            float one = 8192f / 5512;
            var a = new TimedHashes(new Hashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {1}, 1, one),
                    new HashedFingerprint(new[] {1}, 2, 2 * one),
                    new HashedFingerprint(new[] {1}, 0, 0)
                }, one * 3 + FingerprintCount, DateTime.Parse("01/15/2019 10:00:00", dtfi), string.Empty),
                DateTime.Parse("01/15/2019 10:00:00", dtfi));

            var b = new TimedHashes(new Hashes(new List<HashedFingerprint>
                    {
                        new HashedFingerprint(new[] {2}, 1, one),
                        new HashedFingerprint(new[] {2}, 2, 2 * one),
                        new HashedFingerprint(new[] {2}, 0, 0)
                    },
                    one * 3 + FingerprintCount, DateTime.Parse("01/15/2019 10:00:01", dtfi), string.Empty),
                DateTime.Parse("01/15/2019 10:00:01", dtfi));

            Assert.IsTrue(a.MergeWith(b, out var result));
            var mergedHashes = result.Hashes.ToList();
            Assert.AreEqual(0, mergedHashes[0].StartsAt);
            Assert.AreEqual(1f, mergedHashes[1].StartsAt, 0.0001);
            Assert.AreEqual(one, mergedHashes[2].StartsAt, 0.0001);
            Assert.AreEqual(1 + one, mergedHashes[3].StartsAt, 0.0001);
            Assert.AreEqual(2 * one, mergedHashes[4].StartsAt, 0.0001);
            Assert.AreEqual(1 + 2 * one, mergedHashes[5].StartsAt, 0.0001);
        }

        [Test]
        public void ShouldMergeCorrectly()
        {
            float acc = 8192 / 5512f;
            var a = TimedHashes.Empty;
            var dateTime = DateTime.Now;
            var b = new TimedHashes(
                new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {1}, 0, 0)}), acc, dateTime, string.Empty), dateTime);
            var c = new TimedHashes(
                new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {2}, 0, 0f)}), acc, dateTime.AddSeconds(acc), string.Empty),
                dateTime.AddSeconds(acc));
            var d = new TimedHashes(
                new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {3}, 0, 0f)}), acc, dateTime.AddSeconds(2 * acc),
                    string.Empty), dateTime.AddSeconds(2 * acc));

            Assert.IsTrue(a.MergeWith(b, out var x));
            Assert.IsTrue(x.MergeWith(c, out var y));
            Assert.IsTrue(y.MergeWith(d, out var z));

            Assert.AreEqual(dateTime, z.StartsAt);
            Assert.AreEqual(3, z.Hashes.Count);
            Assert.AreEqual(3 * acc, z.TotalSeconds, 0.001);
        }

        [Test]
        public void ShouldMergeLongSequences()
        {
            DateTimeFormatInfo dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            var first = new List<HashedFingerprint>();
            var second = new List<HashedFingerprint>();

            float one = 8192f / 5512;
            for (int i = 0; i < 100; ++i)
            {
                first.Add(new HashedFingerprint(new[] {1}, (uint) i, i * one));
                second.Add(new HashedFingerprint(new[] {2}, (uint) i, i * one));
            }

            var r = new Random();
            var a = new TimedHashes(
                new Hashes(first.OrderBy(x => r.Next()).ToList(), first.Count * one + FingerprintCount, DateTime.Parse("01/15/2019 10:00:00", dtfi),
                    string.Empty), DateTime.Parse("01/15/2019 10:00:00", dtfi));
            var b = new TimedHashes(
                new Hashes(second.OrderBy(x => r.Next()).ToList(), second.Count * one + FingerprintCount,
                    DateTime.Parse("01/15/2019 10:00:01.3", dtfi), string.Empty), DateTime.Parse("01/15/2019 10:00:01.3", dtfi));

            Assert.IsTrue(a.MergeWith(b, out var c));
            for (int i = 0; i < 200; ++i)
            {
                var mergedHashes = c.Hashes.ToList();
                Assert.AreEqual(i, mergedHashes[i].SequenceNumber);
                if (i % 2 == 0)
                {
                    Assert.AreEqual(one * (i / 2), mergedHashes[i].StartsAt, 0.0001);
                    CollectionAssert.AreEqual(new[] {1}, mergedHashes[i].HashBins);
                }
                else
                {
                    Assert.AreEqual(1.3f + one * (i / 2), mergedHashes[i].StartsAt, 0.0001);
                    CollectionAssert.AreEqual(new[] {2}, mergedHashes[i].HashBins);
                }
            }
        }

        [Test]
        public void CantMergeSinceTheGapIsTooBig()
        {
            var dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            var a = new TimedHashes(new Hashes(new List<HashedFingerprint>
                    {
                        new HashedFingerprint(new[] {1}, 0, 0)
                    },
                    FingerprintCount,
                    DateTime.Parse("01/15/2019 10:00:00", dtfi), string.Empty),
                DateTime.Parse("01/15/2019 10:00:00", dtfi));

            var b = new TimedHashes(new Hashes(new List<HashedFingerprint>
                    {
                        new HashedFingerprint(new[] {2}, 0, 0)
                    },
                    FingerprintCount,
                    DateTime.Parse("01/15/2019 10:01:00", dtfi), string.Empty),
                DateTime.Parse("01/15/2019 10:01:00", dtfi));

            Assert.IsFalse(a.MergeWith(b, out _));
        }

        [Test]
        public void CanSerializeAndDeserialize()
        {
            var list = GetHashedFingerprints();

            var timed = new TimedHashes(new Hashes(list, list.Count * FingerprintCount, DateTime.Now, string.Empty), DateTime.Now);
            var buffer = Serialize(timed);
            var deserialized = Deserialize(buffer);
            Assert.AreEqual(timed.Hashes.Count, deserialized.Hashes.Count);
            Assert.AreEqual(timed.StartsAt, deserialized.StartsAt);

            for (int i = 0; i < timed.Hashes.Count; ++i)
            {
                var timedHashes = timed.Hashes.ToList();
                HashedFingerprint a = timedHashes[i];
                var deserializedHashes = deserialized.Hashes.ToList();
                HashedFingerprint b = deserializedHashes[i];

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

                list.Add(new HashedFingerprint(hashes, (uint) (i + 1), i * 1.48f));
            }

            return list;
        }
    }
}