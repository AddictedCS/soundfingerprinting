namespace SoundFingerprinting.Tests.Unit.Query;

using System.IO;
using NUnit.Framework;
using ProtoBuf;
using SoundFingerprinting.Query;

[TestFixture]
public class MatchedWithProtobufTest
{
    [Test]
    public void RoundTripShouldBeByteIdenticalToPreBridgingShape()
    {
        var matched = new MatchedWith(querySequenceNumber: 42, queryMatchAt: 1.5f, trackSequenceNumber: 7, trackMatchAt: 0.5f, score: 3.14);

        byte[] payload1, payload2;
        using (var ms = new MemoryStream())
        {
            Serializer.Serialize(ms, matched);
            payload1 = ms.ToArray();
        }

        var deserialized = Serializer.Deserialize<MatchedWith>(new MemoryStream(payload1));
        using (var ms = new MemoryStream())
        {
            Serializer.Serialize(ms, deserialized);
            payload2 = ms.ToArray();
        }

        Assert.That(payload2, Is.EqualTo(payload1).AsCollection);
        Assert.That(deserialized.QuerySequenceNumber, Is.EqualTo(matched.QuerySequenceNumber));
        Assert.That(deserialized.QueryMatchAt, Is.EqualTo(matched.QueryMatchAt));
        Assert.That(deserialized.TrackSequenceNumber, Is.EqualTo(matched.TrackSequenceNumber));
        Assert.That(deserialized.TrackMatchAt, Is.EqualTo(matched.TrackMatchAt));
        Assert.That(deserialized.Score, Is.EqualTo(matched.Score));
    }
}
