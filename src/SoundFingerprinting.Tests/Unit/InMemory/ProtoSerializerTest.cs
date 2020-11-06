﻿namespace SoundFingerprinting.Tests.Unit.InMemory
{
    using System;
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

            Assert.NotNull(reference);
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

            Assert.AreEqual(sub, deserialized);
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

            Assert.AreEqual(11, deserialized.Next().Get<uint>());
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
                Assert.IsNotNull(data);
                Assert.AreEqual(hashes.Count, data.Count);
                Assert.AreEqual(hashes.DurationInSeconds, data.DurationInSeconds);
                Assert.AreEqual(hashes.StreamId, data.StreamId);
                CollectionAssert.AreEqual(hashes.Origins, data.Origins);
            }
        }
    }
}
