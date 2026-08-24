namespace SoundFingerprinting.Tests.Unit.Data
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Data;

    [TestFixture]
    public class AVHashesTest
    {
        private static readonly DateTime RelativeTo = new (2026, 8, 24, 10, 0, 0, DateTimeKind.Utc);

        [Test]
        public void ShouldCutBothSidesOnAbsoluteTime()
        {
            var hashes = new AVHashes(GetHashes(MediaType.Audio), GetHashes(MediaType.Video));

            var range = hashes.GetRange(RelativeTo.AddSeconds(3), 4f);

            Assert.Multiple(() =>
            {
                Assert.That(range.Audio!.RelativeTo, Is.EqualTo(RelativeTo.AddSeconds(3)));
                Assert.That(range.Video!.RelativeTo, Is.EqualTo(RelativeTo.AddSeconds(3)));
                Assert.That(range.Audio!.Select(_ => _.StartsAt), Is.EqualTo(new[] { 0f, 1f, 2f, 3f }));
                Assert.That(range.Video!.Select(_ => _.StartsAt), Is.EqualTo(new[] { 0f, 1f, 2f, 3f }));
            });
        }

        [Test]
        public void ShouldNotCarryTheFingerprintingTimeOfTheParent()
        {
            var hashes = new AVHashes(GetHashes(MediaType.Audio), GetHashes(MediaType.Video), new AVFingerprintingTime(100, 200));

            var range = hashes.GetRange(RelativeTo, 2f);

            Assert.Multiple(() =>
            {
                Assert.That(range.FingerprintingTime.AudioMilliseconds, Is.Zero);
                Assert.That(range.FingerprintingTime.VideoMilliseconds, Is.Zero);
            });
        }

        [Test]
        public void ShouldKeepAMissingSideMissing()
        {
            var audioOnly = new AVHashes(GetHashes(MediaType.Audio), null);

            var range = audioOnly.GetRange(RelativeTo, 2f);

            Assert.Multiple(() =>
            {
                Assert.That(range.Video, Is.Null);
                Assert.That(range.Audio!.IsEmpty, Is.False);
            });
        }

        [Test]
        public void ShouldReturnAnEmptyRangeWhenNothingFallsInTheWindow()
        {
            var hashes = new AVHashes(GetHashes(MediaType.Audio), GetHashes(MediaType.Video));

            var range = hashes.GetRange(RelativeTo.AddSeconds(100), 4f);

            Assert.That(range.IsEmpty, Is.True);
        }

        private static Hashes GetHashes(MediaType mediaType)
        {
            // ten one-second fingerprints, so a fingerprint covers exactly [StartsAt, StartsAt + 1)
            var fingerprints = Enumerable
                .Range(0, 10)
                .Select(sequenceNumber => new HashedFingerprint(new[] { 255 }, (uint)sequenceNumber, sequenceNumber, new byte[] { 1 }));
            return new Hashes(fingerprints, 10, mediaType, RelativeTo);
        }
    }
}
