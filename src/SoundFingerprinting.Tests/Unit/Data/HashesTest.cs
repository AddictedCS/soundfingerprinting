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
    public class HashesTest
    {
        private const double FingerprintCount = 8192.0d / 5512;

        [Test]
        public void ShouldMerge()
        {
            var dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            float one = 8192f / 5512;
            var a = new Hashes(new List<HashedFingerprint>
            {
                new HashedFingerprint(new[] {1}, 1, one, Array.Empty<byte>()),
                new HashedFingerprint(new[] {1}, 2, 2 * one, Array.Empty<byte>()),
                new HashedFingerprint(new[] {1}, 0, 0, Array.Empty<byte>())
            }, one * 3 + FingerprintCount, DateTime.Parse("01/15/2019 10:00:00", dtfi), Enumerable.Empty<string>());

            var b = new Hashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {2}, 1, one, Array.Empty<byte>()),
                    new HashedFingerprint(new[] {2}, 2, 2 * one, Array.Empty<byte>()),
                    new HashedFingerprint(new[] {2}, 0, 0, Array.Empty<byte>())
                },
                one * 3 + FingerprintCount, DateTime.Parse("01/15/2019 10:00:01", dtfi), Enumerable.Empty<string>());

            Assert.IsTrue(a.MergeWith(b, out var result));
            var mergedHashes = result.ToList();
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
            var a = Hashes.Empty;
            var dateTime = DateTime.Now;
            var b = new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {1}, 0, 0f, Array.Empty<byte>())}), acc, dateTime, Enumerable.Empty<string>());
            var c = new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {2}, 0, 0f, Array.Empty<byte>())}), acc, dateTime.AddSeconds(acc), Enumerable.Empty<string>());
            var d = new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {3}, 0, 0f, Array.Empty<byte>())}), acc, dateTime.AddSeconds(2 * acc), Enumerable.Empty<string>());

            Assert.IsTrue(a.MergeWith(b, out var x));
            Assert.IsTrue(x.MergeWith(c, out var y));
            Assert.IsTrue(y.MergeWith(d, out var z));

            Assert.AreEqual(dateTime, z.RelativeTo);
            Assert.AreEqual(3, z.Count);
            Assert.AreEqual(3 * acc, z.DurationInSeconds, 0.001);
        }

        [Test]
        public void ShouldMergeLongSequences()
        {
            var dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            var first = new List<HashedFingerprint>();
            var second = new List<HashedFingerprint>();

            float one = 8192f / 5512;
            for (int i = 0; i < 100; ++i)
            {
                first.Add(new HashedFingerprint(new[] {1}, (uint) i, i * one, Array.Empty<byte>()));
                second.Add(new HashedFingerprint(new[] {2}, (uint) i, i * one, Array.Empty<byte>()));
            }

            var r = new Random();
            var a = new Hashes(first.OrderBy(x => r.Next()).ToList(), first.Count * one + FingerprintCount, DateTime.Parse("01/15/2019 10:00:00", dtfi), Enumerable.Empty<string>());
            var b = new Hashes(second.OrderBy(x => r.Next()).ToList(), second.Count * one + FingerprintCount, DateTime.Parse("01/15/2019 10:00:01.3", dtfi), Enumerable.Empty<string>());

            Assert.IsTrue(a.MergeWith(b, out var c));
            for (int i = 0; i < 200; ++i)
            {
                var mergedHashes = c.ToList();
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
            var a = new Hashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {1}, 0, 0, Array.Empty<byte>())
                },
                FingerprintCount,
                DateTime.Parse("01/15/2019 10:00:00", dtfi), Enumerable.Empty<string>());

            var b = new Hashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {2}, 0, 0, Array.Empty<byte>())
                },
                FingerprintCount,
                DateTime.Parse("01/15/2019 10:01:00", dtfi), Enumerable.Empty<string>());

            Assert.IsFalse(a.MergeWith(b, out _));
        }

        [Test]
        public void CanSerializeAndDeserialize()
        {
            var list = GetHashedFingerprints();

            var timed = new Hashes(list, list.Count * FingerprintCount, DateTime.Now, Enumerable.Empty<string>());
            var buffer = Serialize(timed);
            var deserialized = Deserialize(buffer);
            Assert.AreEqual(timed.Count, deserialized.Count);
            Assert.AreEqual(timed.RelativeTo, deserialized.RelativeTo);

            for (int i = 0; i < timed.Count; ++i)
            {
                var timedHashes = timed.ToList();
                HashedFingerprint a = timedHashes[i];
                var deserializedHashes = deserialized.ToList();
                HashedFingerprint b = deserializedHashes[i];

                Assert.AreEqual(a.StartsAt, b.StartsAt);
                Assert.AreEqual(a.SequenceNumber, b.SequenceNumber);
                CollectionAssert.AreEqual(a.HashBins, b.HashBins);
            }
        }

        [Test]
        public void ShouldMergeNonOverlappingSequences()
        {
            var dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            int count = 80;
            var aStartsAt = DateTime.Parse("01/15/2019 10:00:00", dtfi);
            var a = new Hashes(GetHashedFingerprints(count), count * 1.48f + 1.48f, aStartsAt);
            var bStartsAt = DateTime.Parse("01/15/2019 10:02:00", dtfi);
            var b = new Hashes(GetHashedFingerprints(count), count * 1.48f + 1.48f, bStartsAt);
            
            Assert.IsTrue(a.MergeWith(b, out var c));
            Assert.AreEqual(count * 2, c.Count);
            var rangeA = c.GetRange(aStartsAt, 120);
            AssertHashesAreEqual(a, rangeA);
            var rangeB = c.GetRange(bStartsAt, 120);
            AssertHashesAreEqual(b, rangeB);
        }

        private static void AssertHashesAreEqual(Hashes a, Hashes b)
        {
            Assert.AreEqual(a.Count, b.Count);
            foreach (var tuple in a.Zip(b))
            {
                CollectionAssert.AreEqual(tuple.First.HashBins, tuple.Second.HashBins);
            }
        }

        private static Hashes Deserialize(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);
            return Serializer.DeserializeWithLengthPrefix<Hashes>(stream, PrefixStyle.Fixed32);
        }

        private static byte[] Serialize(Hashes timed)
        {
            using var stream = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(stream, timed, PrefixStyle.Fixed32);
            return stream.ToArray();
        }

        private static List<HashedFingerprint> GetHashedFingerprints(int count = 100)
        {
            var random = new Random();
            var list = new List<HashedFingerprint>();
            for (int i = 0; i < count; ++i)
            {
                int[] hashes = new int[25];
                for (int j = 0; j < 25; ++j)
                {
                    int hash = random.Next();
                    hashes[j] = hash;
                }

                list.Add(new HashedFingerprint(hashes, (uint) (i + 1), i * 1.48f, Array.Empty<byte>()));
            }

            return list;
        }
    }
}