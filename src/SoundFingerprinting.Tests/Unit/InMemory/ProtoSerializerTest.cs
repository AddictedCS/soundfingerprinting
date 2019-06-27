namespace SoundFingerprinting.Tests.Unit.InMemory
{
    using System.IO;

    using NUnit.Framework;
    using ProtoBuf;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    public class ProtoSerializerTest
    {
        [Test]
        public void ShouldSerializeModelReference()
        {
            var @ref = new ModelReference<int>(42);

            using (var stream = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix<IModelReference>(stream, @ref, PrefixStyle.Fixed32);
                byte[] serialized = stream.ToArray();
                using (var deserializedStream = new MemoryStream(serialized))
                {
                    var reference = Serializer.DeserializeWithLengthPrefix<IModelReference>(deserializedStream, PrefixStyle.Fixed32);

                    Assert.NotNull(reference);
                }
            }
        }

        [Test]
        public void ShouldSerialize()
        {
            var sub = new SubFingerprintData(new[] { 1, 2, 3 }, 1, 1f, new[] { "1", "2" }, new ModelReference<int>(1), new ModelReference<int>(2));

            using (var stream = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(stream, sub, PrefixStyle.Fixed32);
                byte[] serialized = stream.ToArray();

                using (var streamFinal = new MemoryStream(serialized))
                {
                    var deserialized = Serializer.DeserializeWithLengthPrefix<SubFingerprintData>(streamFinal, PrefixStyle.Fixed32);

                    Assert.AreEqual(sub, deserialized);
                }
            }
        }

        [Test]
        public void ShouldSerializeModelReferenceProviders()
        {
            var provider = new UIntModelReferenceProvider(10);
            using (var stream = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(stream, provider, PrefixStyle.Fixed32);
                byte[] serialized = stream.ToArray();
                using (var streamFinal = new MemoryStream(serialized))
                {
                    var deserialized = Serializer.DeserializeWithLengthPrefix<UIntModelReferenceProvider>(streamFinal, PrefixStyle.Fixed32);

                    Assert.AreEqual(11, (uint)deserialized.Next().Id);
                }
            }
        }
    }
}
