namespace SoundFingerprinting.Tests.Unit
{
    using System.IO;
    using NUnit.Framework;
    using ProtoBuf;

    [TestFixture]
    public class ModelServiceInfoTest
    {
        [Test]
        public void ShouldHaveAHumanReadableStringRepresentation()
        {
            var info = new ModelServiceInfo("model-service", 1, 2, new[] { 3, 4, 5 });

			Assert.That(info.ToString(), Is.EqualTo("ModelServiceInfo{Id: model-service, TracksCount: 1, SubFingerprintsCount: 2, HashCountsInTables: [3, 4, 5]}"));
        }

        [TestCase(null)]
        [TestCase(0L)]
        [TestCase(123456789L)]
        public void SequenceRoundTripsIncludingZeroAndNull(long? sequence)
        {
            var info = new ModelServiceInfo("model-service", 1, 2, new[] { 3 }, sequence);

            var roundTripped = RoundTrip(info);

            Assert.That(roundTripped.Sequence, Is.EqualTo(sequence));
            Assert.That(roundTripped.Id, Is.EqualTo("model-service"));
        }

        [Test]
        public void APayloadWithoutASequenceReadsNull()
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, new LegacyModelServiceInfo { TracksCount = 1, SubFingerprintsCount = 2, HashCountsInTables = new[] { 3 }, Id = "legacy" });
            stream.Position = 0;

            var info = Serializer.Deserialize<ModelServiceInfo>(stream);

            Assert.That(info.Sequence, Is.Null);
            Assert.That(info.TracksCount, Is.EqualTo(1));
        }

        private static ModelServiceInfo RoundTrip(ModelServiceInfo info)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, info);
            stream.Position = 0;
            return Serializer.Deserialize<ModelServiceInfo>(stream);
        }

        // the wire shape of the type before the sequence member existed
        [ProtoContract]
        private class LegacyModelServiceInfo
        {
            [ProtoMember(1)]
            public int TracksCount { get; set; }

            [ProtoMember(2)]
            public int SubFingerprintsCount { get; set; }

            [ProtoMember(3)]
            public int[] HashCountsInTables { get; set; }

            [ProtoMember(4)]
            public string Id { get; set; }
        }
    }
}
