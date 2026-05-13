namespace SoundFingerprinting.Tests.Unit.Audio;

using System.Collections.Generic;
using NUnit.Framework;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Data;

[TestFixture]
public class SpectralProfileExtensionsTest
{
    [Test]
    public void WithMetaFieldsFromHashesShouldPropagateSpectralProfileWhenAbsentOnTrack()
    {
        var profile = new SpectralProfile(new List<SpectralSecond> { new (0.5, 0.5) });
        var encoded = SpectralProfileCodecRegistry.Default.Encode(profile);
        var hashes = new AVHashes(MakeHashes(encoded), null);
        var track = new TrackInfo("id-1", "t", "a");

        var augmented = track.WithMetaFieldsFromHashes(hashes);

        Assert.That(augmented, Is.Not.SameAs(track));
        Assert.That(augmented.MetaFields.TryGetValue(SpectralProfileKeys.SpectralProfile, out var got), Is.True);
        Assert.That(got, Is.EqualTo(encoded));
    }

    [Test]
    public void WithMetaFieldsFromHashesShouldPreserveExistingMetaFieldsOnCollision()
    {
        var fromHashes = "FROM-HASHES";
        var fromTrack = "FROM-TRACK";
        var hashes = new AVHashes(MakeHashes(fromHashes), null);
        var existingMeta = new Dictionary<string, string>
        {
            [SpectralProfileKeys.SpectralProfile] = fromTrack,
        };
        var track = new TrackInfo("id-1", "t", "a", existingMeta);

        var augmented = track.WithMetaFieldsFromHashes(hashes);

        Assert.That(augmented, Is.SameAs(track));
        Assert.That(augmented.MetaFields[SpectralProfileKeys.SpectralProfile], Is.EqualTo(fromTrack));
    }

    [Test]
    public void WithMetaFieldsFromHashesShouldReturnSameInstanceWhenHashesCarryNoProfile()
    {
        var hashes = new AVHashes(MakeHashes(properties: null), null);
        var track = new TrackInfo("id-1", "t", "a");

        var augmented = track.WithMetaFieldsFromHashes(hashes);

        Assert.That(augmented, Is.SameAs(track));
        Assert.That(augmented.MetaFields, Does.Not.ContainKey(SpectralProfileKeys.SpectralProfile));
    }

    [Test]
    public void WithMetaFieldsFromHashesShouldReturnSameInstanceWhenAudioIsNull()
    {
        var videoOnly = new AVHashes(null, new Hashes(new List<HashedFingerprint>(), 30d, MediaType.Video));
        var track = new TrackInfo("id-1", "t", "a");

        var augmented = track.WithMetaFieldsFromHashes(videoOnly);

        Assert.That(augmented, Is.SameAs(track));
    }

    [Test]
    public void WithMetaFieldsFromHashesShouldNotMutateOriginalMetaFields()
    {
        var encoded = "ENCODED";
        var hashes = new AVHashes(MakeHashes(encoded), null);
        var existingMeta = new Dictionary<string, string> { ["custom"] = "value" };
        var track = new TrackInfo("id-1", "t", "a", existingMeta);

        var augmented = track.WithMetaFieldsFromHashes(hashes);

        Assert.That(existingMeta, Does.Not.ContainKey(SpectralProfileKeys.SpectralProfile));
        Assert.That(augmented.MetaFields[SpectralProfileKeys.SpectralProfile], Is.EqualTo(encoded));
        Assert.That(augmented.MetaFields["custom"], Is.EqualTo("value"));
    }

    [Test]
    public void WithMetaFieldsFromHashesShouldAcceptSingleHashesOverload()
    {
        var encoded = "ENCODED-SINGLE";
        var audioOnly = MakeHashes(encoded);
        var track = new TrackInfo("id-1", "t", "a");

        var augmented = track.WithMetaFieldsFromHashes(audioOnly);

        Assert.That(augmented.MetaFields[SpectralProfileKeys.SpectralProfile], Is.EqualTo(encoded));
    }

    [Test]
    public void WithMetaFieldsFromHashesShouldBeNoOpForNullSingleHashes()
    {
        var track = new TrackInfo("id-1", "t", "a");

        var augmented = track.WithMetaFieldsFromHashes((Hashes?)null);

        Assert.That(augmented, Is.SameAs(track));
    }

    [Test]
    public void WithPropertiesFromTrackShouldPropagateSpectralProfileWhenAbsentOnHashes()
    {
        var encoded = "FROM-TRACK";
        var meta = new Dictionary<string, string> { [SpectralProfileKeys.SpectralProfile] = encoded };
        var track = new TrackInfo("id-1", "t", "a", meta);
        var hashes = MakeHashes(properties: null);

        var augmented = hashes.WithPropertiesFromTrack(track);

        Assert.That(augmented!.Properties[SpectralProfileKeys.SpectralProfile], Is.EqualTo(encoded));
    }

    [Test]
    public void WithPropertiesFromTrackShouldPreserveExistingHashesPropertiesOnCollision()
    {
        var fromHashes = "FROM-HASHES";
        var fromTrack = "FROM-TRACK";
        var meta = new Dictionary<string, string> { [SpectralProfileKeys.SpectralProfile] = fromTrack };
        var track = new TrackInfo("id-1", "t", "a", meta);
        var hashes = MakeHashes(fromHashes);

        var augmented = hashes.WithPropertiesFromTrack(track);

        Assert.That(augmented!.Properties[SpectralProfileKeys.SpectralProfile], Is.EqualTo(fromHashes));
    }

    [Test]
    public void WithPropertiesFromTrackShouldNotAddKeyWhenTrackHasNoWellKnownKeys()
    {
        var track = new TrackInfo("id-1", "t", "a");
        var hashes = MakeHashes(properties: null);

        var augmented = hashes.WithPropertiesFromTrack(track);

        Assert.That(augmented!.Properties, Does.Not.ContainKey(SpectralProfileKeys.SpectralProfile));
    }

    [Test]
    public void WithPropertiesFromTrackShouldBeNoOpForNullHashes()
    {
        var meta = new Dictionary<string, string> { [SpectralProfileKeys.SpectralProfile] = "x" };
        var track = new TrackInfo("id-1", "t", "a", meta);

        var augmented = ((Hashes?)null).WithPropertiesFromTrack(track);

        Assert.That(augmented, Is.Null);
    }

    [Test]
    public void WithPropertiesFromMetaFieldsShouldPropagateFromDictionary()
    {
        var encoded = "FROM-META";
        var meta = new Dictionary<string, string> { [SpectralProfileKeys.SpectralProfile] = encoded };
        var hashes = MakeHashes(properties: null);

        var augmented = hashes.WithPropertiesFromMetaFields(meta);

        Assert.That(augmented!.Properties[SpectralProfileKeys.SpectralProfile], Is.EqualTo(encoded));
    }

    [Test]
    public void WithPropertiesFromMetaFieldsShouldBeNoOpForNullMetaFields()
    {
        var hashes = MakeHashes(properties: null);

        var augmented = hashes.WithPropertiesFromMetaFields(null);

        Assert.That(augmented, Is.SameAs(hashes));
    }

    private static Hashes MakeHashes(string? properties)
    {
        var hashes = new Hashes(new List<HashedFingerprint>(), 30d, MediaType.Audio);
        return properties == null ? hashes : hashes.WithProperty(SpectralProfileKeys.SpectralProfile, properties);
    }
}
