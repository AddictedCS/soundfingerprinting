namespace SoundFingerprinting.Tests.Unit.LCS;

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoBuf;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;

[TestFixture]
public class CoverageProtobufTest
{
    [Test]
    public void ShouldRoundTripBridgedSecondsAcrossWire()
    {
        var original = new Coverage(
            bestPath: new[] { new MatchedWith(0, 0, 0, 0, score: 1) },
            queryLength: 30,
            trackLength: 30,
            fingerprintLength: 1.5,
            permittedGap: 0,
            bridgedSeconds: 42);

        using var ms = new MemoryStream();
        Serializer.Serialize(ms, original);
        ms.Position = 0;
        var roundTripped = Serializer.Deserialize<Coverage>(ms);

        Assert.That(roundTripped.BridgedSeconds, Is.EqualTo(42));
    }

    [Test]
    public void LegacyPayloadWithoutTag6ShouldDeserialiseWithDefaultBridgedSeconds()
    {
        // Serialise a stand-in type whose ProtoContract matches Coverage's tags 1..5 but omits tag 6.
        // proto-net's wire format means the resulting bytes are a valid Coverage encoding with no tag 6 present,
        // simulating a payload produced by code that predates the BridgedSeconds addition.
        var legacy = new LegacyCoverageShape
        {
            QueryLength = 30,
            TrackLength = 30,
            BestPath = new List<MatchedWith> { new (0, 0, 0, 0, score: 1) },
            FingerprintLength = 1.5,
            PermittedGap = 0,
        };

        using var ms = new MemoryStream();
        Serializer.Serialize(ms, legacy);
        ms.Position = 0;
        var deserialised = Serializer.Deserialize<Coverage>(ms);

        Assert.That(deserialised.BridgedSeconds, Is.EqualTo(0), "missing tag 6 on the wire must deserialise to default(int)");
        Assert.That(deserialised.QueryLength, Is.EqualTo(30));
        Assert.That(deserialised.TrackLength, Is.EqualTo(30));
        Assert.That(deserialised.FingerprintLength, Is.EqualTo(1.5));
    }

    [ProtoContract(SkipConstructor = true)]
    private sealed class LegacyCoverageShape
    {
        [ProtoMember(1)]
        public double QueryLength { get; set; }

        [ProtoMember(2)]
        public double TrackLength { get; set; }

        [ProtoMember(3)]
        public IEnumerable<MatchedWith>? BestPath { get; set; }

        [ProtoMember(4)]
        public double FingerprintLength { get; set; }

        [ProtoMember(5)]
        public double PermittedGap { get; set; }
    }
}
