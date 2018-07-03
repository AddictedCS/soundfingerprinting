namespace SoundFingerprinting.Tests.Unit.InMemory
{
    using System.IO;

    using System.Collections.Generic;

    using NUnit.Framework;
    using ProtoBuf;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    public class ProtoSerializerTest
    {
        [Test]
        public void ShouldSerialize()
        {
            var sub = new SubFingerprintData(
                          new[] { 1, 2, 3 },
                          1,
                          1f,
                          new ModelReference<int>(1),
                          new ModelReference<int>(2)) { Clusters = new List<string>() { "1", "2" } };

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
    }
}
