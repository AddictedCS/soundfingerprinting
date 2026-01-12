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
            }, one * 3 + FingerprintCount, MediaType.Audio, DateTime.Parse("01/15/2019 10:00:00", dtfi), Enumerable.Empty<string>());

            var b = new Hashes(new List<HashedFingerprint>
                {
                    new HashedFingerprint(new[] {2}, 1, one, Array.Empty<byte>()),
                    new HashedFingerprint(new[] {2}, 2, 2 * one, Array.Empty<byte>()),
                    new HashedFingerprint(new[] {2}, 0, 0, Array.Empty<byte>())
                },
                one * 3 + FingerprintCount, MediaType.Audio, DateTime.Parse("01/15/2019 10:00:01", dtfi), Enumerable.Empty<string>());

            var result = a.MergeWith(b);
            var mergedHashes = result.ToList();
			Assert.Multiple(() =>
			{
				Assert.That(mergedHashes[0].StartsAt, Is.EqualTo(0));
				Assert.That(mergedHashes[1].StartsAt, Is.EqualTo(1f).Within(0.0001));
				Assert.That(mergedHashes[2].StartsAt, Is.EqualTo(one).Within(0.0001));
				Assert.That(mergedHashes[3].StartsAt, Is.EqualTo(1 + one).Within(0.0001));
				Assert.That(mergedHashes[4].StartsAt, Is.EqualTo(2 * one).Within(0.0001));
				Assert.That(mergedHashes[5].StartsAt, Is.EqualTo(1 + 2 * one).Within(0.0001));
			});
		}

        [Test]
        public void ShouldMergeCorrectly()
        {
            float acc = 8192 / 5512f;
            var a = Hashes.GetEmpty(MediaType.Audio);
            var dateTime = DateTime.Now;
            var b = new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {1}, 0, 0f, Array.Empty<byte>())}), acc, MediaType.Audio, dateTime, Enumerable.Empty<string>());
            var c = new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {2}, 0, 0f, Array.Empty<byte>())}), acc, MediaType.Audio, dateTime.AddSeconds(acc), Enumerable.Empty<string>());
            var d = new Hashes(new List<HashedFingerprint>(new[] {new HashedFingerprint(new[] {3}, 0, 0f, Array.Empty<byte>())}), acc, MediaType.Audio, dateTime.AddSeconds(2 * acc), Enumerable.Empty<string>());

            var x = a.MergeWith(b);
            var y = x.MergeWith(c);
            var z = y.MergeWith(d);

			Assert.Multiple(() =>
			{
				Assert.That(z.RelativeTo, Is.EqualTo(dateTime));
				Assert.That(z, Has.Count.EqualTo(3));
			});
			Assert.That(z.DurationInSeconds, Is.EqualTo(3 * acc).Within(0.001));
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
            var a = new Hashes(first.OrderBy(x => r.Next()).ToList(), first.Count * one + FingerprintCount, MediaType.Audio, DateTime.Parse("01/15/2019 10:00:00", dtfi), Enumerable.Empty<string>());
            var b = new Hashes(second.OrderBy(x => r.Next()).ToList(), second.Count * one + FingerprintCount, MediaType.Audio, DateTime.Parse("01/15/2019 10:00:01.3", dtfi), Enumerable.Empty<string>());

            var c = a.MergeWith(b);
            for (int i = 0; i < 200; ++i)
            {
                var mergedHashes = c.ToList();
				Assert.That(mergedHashes[i].SequenceNumber, Is.EqualTo(i));
                if (i % 2 == 0)
                {
					Assert.That(mergedHashes[i].StartsAt, Is.EqualTo(one * (i / 2)).Within(0.0001));
					Assert.That(mergedHashes[i].HashBins, Is.EqualTo(new[] { 1 }).AsCollection);
                }
                else
                {
					Assert.That(mergedHashes[i].StartsAt, Is.EqualTo(1.3f + one * (i / 2)).Within(0.0001));
					Assert.That(mergedHashes[i].HashBins, Is.EqualTo(new[] { 2 }).AsCollection);
                }
            }
        }

        [Test]
        public void CanSerializeAndDeserialize()
        {
            var list = GetHashedFingerprints();

            var timed = new Hashes(list, list.Count * FingerprintCount, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>());
            var buffer = Serialize(timed);
            var deserialized = Deserialize(buffer);
			Assert.That(deserialized, Has.Count.EqualTo(timed.Count));
			Assert.That(deserialized.RelativeTo, Is.EqualTo(timed.RelativeTo));

            for (int i = 0; i < timed.Count; ++i)
            {
                var timedHashes = timed.ToList();
                HashedFingerprint a = timedHashes[i];
                var deserializedHashes = deserialized.ToList();
                HashedFingerprint b = deserializedHashes[i];

				Assert.Multiple(() =>
				{
					Assert.That(b.StartsAt, Is.EqualTo(a.StartsAt));
					Assert.That(b.SequenceNumber, Is.EqualTo(a.SequenceNumber));
				});
				Assert.That(b.HashBins, Is.EqualTo(a.HashBins).AsCollection);
            }
        }

        [Test]
        public void ShouldMergeNonOverlappingSequences()
        {
            var dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            int count = 80;
            var aStartsAt = DateTime.Parse("01/15/2019 10:00:00", dtfi);
            var a = new Hashes(GetHashedFingerprints(count), count * 1.48f, MediaType.Audio, aStartsAt);
            var bStartsAt = DateTime.Parse("01/15/2019 10:02:00", dtfi);
            var b = new Hashes(GetHashedFingerprints(count), count * 1.48f, MediaType.Audio, bStartsAt);
            
            var c = a.MergeWith(b);
			Assert.That(c, Has.Count.EqualTo(count * 2));
            
            AssertInvariantsForHashes(c, aStartsAt);
            var rangeA = c.GetRange(aStartsAt, 120);
            AssertHashesAreEqual(a, rangeA);
            AssertInvariantsForHashes(rangeA, aStartsAt);
            var rangeB = c.GetRange(bStartsAt, 120);
            AssertHashesAreEqual(b, rangeB);
            AssertInvariantsForHashes(rangeB, bStartsAt);
        }
        
        [Test]
        public void ShouldReturnRangeHashes()
        {
            var dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            int count = 80;
            var aStartsAt = DateTime.Parse("01/15/2019 10:00:00", dtfi);
            var a = new Hashes(GetHashedFingerprints(count), count * 1.48f, MediaType.Audio, aStartsAt);
            var bStartsAt = DateTime.Parse("01/15/2019 10:01:58.4", dtfi);
            var b = new Hashes(GetHashedFingerprints(count), count * 1.48f, MediaType.Audio, bStartsAt);
            
            var c = a.MergeWith(b);
			Assert.That(c, Has.Count.EqualTo(count * 2));
            
            var rangeA = c.GetRange(0, count * 1.48f);
            AssertHashesAreEqual(a, rangeA);
            AssertInvariantsForHashes(rangeA, aStartsAt);
            var rangeB = c.GetRange(count * 1.48f, count * 1.48f);
            AssertHashesAreEqual(b, rangeB);
            AssertInvariantsForHashes(rangeB, bStartsAt);
        }

        [Test]
        public void ShouldMergeCorrectlyRealtimeHashes()
        {
            var prev = TestUtilities.GetRandomHashes(200).WithRelativeTo(DateTime.UnixEpoch);
            var next = TestUtilities.GetRandomHashes(205).WithRelativeTo(DateTime.UnixEpoch.AddSeconds(195)).WithTimeOffset(-5);

            var merged =  prev.MergeWith(next);

			Assert.That(merged.DurationInSeconds, Is.EqualTo(400));
        }

        private static void AssertInvariantsForHashes(Hashes hashes, DateTime startsAt)
        {
			Assert.That((startsAt - hashes.RelativeTo).TotalSeconds, Is.EqualTo(0).Within(0.1));
            var list = hashes.ToList();
			Assert.Multiple(() =>
			{
				Assert.That(list.First().StartsAt, Is.EqualTo(0));
				Assert.That(list.First().SequenceNumber, Is.EqualTo(0));
			});
			for (int i = 1; i < hashes.Count; ++i)
            {
				Assert.Multiple(() =>
				{
					Assert.That(list[i].StartsAt >= list[i - 1].StartsAt, Is.True);
					Assert.That(list[i].SequenceNumber >= list[i - 1].SequenceNumber, Is.True);
				});
			}

			Assert.That(list.Last().StartsAt - list.First().StartsAt + 1.48f, Is.EqualTo(hashes.DurationInSeconds).Within(0.1f));
        }

        private static void AssertHashesAreEqual(Hashes a, Hashes b)
        {
			Assert.That(b, Has.Count.EqualTo(a.Count));
            foreach (var tuple in a.Zip(b))
            {
				Assert.That(tuple.Second.HashBins, Is.EqualTo(tuple.First.HashBins).AsCollection);
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