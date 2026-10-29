namespace SoundFingerprinting.Tests.Unit.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NUnit.Framework;
    using ProtoBuf;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    [TestFixture]
    public class ProtoSerializerTest
    {
        [Test]
        public void ShouldSerializeModelReference()
        {
            var @ref = new ModelReference<int>(42);

            using var stream = new MemoryStream();
            Serializer.SerializeWithLengthPrefix<IModelReference>(stream, @ref, PrefixStyle.Fixed32);
            byte[] serialized = stream.ToArray();
            using var deserializedStream = new MemoryStream(serialized);
            var reference = Serializer.DeserializeWithLengthPrefix<IModelReference>(deserializedStream, PrefixStyle.Fixed32);

			Assert.That(reference, Is.Not.Null);
        }

        [Test]
        public void ShouldSerialize()
        {
            var sub = new SubFingerprintData(new[] { 1, 2, 3 }, 1, 1f, new ModelReference<int>(1), new ModelReference<int>(2));

            using var stream = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(stream, sub, PrefixStyle.Fixed32);
            byte[] serialized = stream.ToArray();

            using var streamFinal = new MemoryStream(serialized);
            var deserialized = Serializer.DeserializeWithLengthPrefix<SubFingerprintData>(streamFinal, PrefixStyle.Fixed32);

			Assert.That(deserialized, Is.EqualTo(sub));
        }

        [Test]
        public void ShouldSerializeModelReferenceProviders()
        {
            var provider = new UIntModelReferenceProvider(10);
            using var stream = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(stream, provider, PrefixStyle.Fixed32);
            byte[] serialized = stream.ToArray();
            using var streamFinal = new MemoryStream(serialized);
            var deserialized = Serializer.DeserializeWithLengthPrefix<UIntModelReferenceProvider>(streamFinal, PrefixStyle.Fixed32);

			Assert.That(deserialized.Next().Get<uint>(), Is.EqualTo(11));
        }

        [Test]
        public void ShouldSerializeHashes()
        {
            var fingerprints = new[] {new HashedFingerprint(new[] {1, 2, 3, 4, 5}, 0, 0, Array.Empty<byte>())};
            var origins = new[] {"test"};
            var hashes = new Hashes(fingerprints, 1.48, MediaType.Audio, DateTime.Now, origins, "CNN");

            byte[] serialized;
            using (var stream = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(stream, hashes, PrefixStyle.Fixed32);
                serialized = stream.ToArray();
            }

            using (var stream = new MemoryStream(serialized))
            {
                var data = Serializer.DeserializeWithLengthPrefix<Hashes>(stream, PrefixStyle.Fixed32);
				Assert.That(data, Is.Not.Null);
				Assert.That(data, Has.Count.EqualTo(hashes.Count));
				Assert.Multiple(() =>
				{
					Assert.That(data.DurationInSeconds, Is.EqualTo(hashes.DurationInSeconds));
					Assert.That(data.StreamId, Is.EqualTo(hashes.StreamId));
				});
				Assert.That(data.Origins, Is.EqualTo(hashes.Origins).AsCollection);
            }
        }

        [Test]
        public void ShouldRoundTripTrackInfoMetaFields()
        {
            var metaFields = new Dictionary<string, string> { { "station", "CNN" }, { "empty", string.Empty } };
            var track = new TrackInfo("id", "title", "artist", metaFields, MediaType.Video);

            var deserialized = RoundTrip(track);

            Assert.That(deserialized.MetaFields, Is.EqualTo(metaFields).AsCollection);
            Assert.That(deserialized.MediaType, Is.EqualTo(MediaType.Video));
        }

        [Test]
        public void ShouldRoundTripEmptyMetaFieldsAsEmpty()
        {
            var track = new TrackInfo("id", "title", "artist", new Dictionary<string, string>(), MediaType.Audio);

            var deserialized = RoundTrip(track);

            Assert.That(deserialized.MetaFields, Is.Not.Null);
            Assert.That(deserialized.MetaFields, Is.Empty);
        }

        [Test]
        public void ShouldRoundTripTrackDataMetaFields()
        {
            var metaFields = new Dictionary<string, string> { { "station", "CNN" } };
            var track = new TrackData("id", "artist", "title", 120d, new ModelReference<uint>(7), metaFields, MediaType.Audio);

            var deserialized = RoundTrip(track);

            Assert.That(deserialized.MetaFields, Is.EqualTo(metaFields).AsCollection);
            Assert.That(deserialized.TrackReference, Is.EqualTo(track.TrackReference));
        }

        [Test]
        public void ShouldRoundTripNullMetaFieldValueAsEmptyString()
        {
            var metaFields = new Dictionary<string, string> { { "station", null! } };
            var track = new TrackInfo("id", "title", "artist", metaFields, MediaType.Audio);

            var deserialized = RoundTrip(track);

            Assert.That(deserialized.MetaFields, Has.Count.EqualTo(1));
            Assert.That(deserialized.MetaFields["station"], Is.EqualTo(string.Empty));
        }

        private static T RoundTrip<T>(T value)
        {
            using var stream = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(stream, value, PrefixStyle.Fixed32);
            stream.Position = 0;
            return Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
        }
    }
}
