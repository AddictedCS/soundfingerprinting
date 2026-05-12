namespace SoundFingerprinting.Tests.Unit.Data;

using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Data;

[TestFixture]
public class HashesMergeSpectralProfileTest
{
    [Test]
    public void ShouldConcatProfilesWhenBothSidesCarryItAndMergeIsContiguous()
    {
        var leftProfile = new SpectralProfile(new List<SpectralSecond> { new (0.10, 0.20) });
        var rightProfile = new SpectralProfile(new List<SpectralSecond> { new (0.50, 0.60) });
        var leftPayload = SpectralProfileCodecRegistry.Default.Encode(leftProfile);
        var rightPayload = SpectralProfileCodecRegistry.Default.Encode(rightProfile);

        var t = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var left = MakeHashes(t, durationSeconds: 1.0, profilePayload: leftPayload);
        var right = MakeHashes(t.AddSeconds(1), durationSeconds: 1.0, profilePayload: rightPayload);

        var merged = left.MergeWith(right);

        Assert.That(merged.Properties.ContainsKey(SpectralProfileKeys.SpectralProfile), Is.True);
        var decoded = SpectralProfileCodecRegistry.Default.Decode(merged.Properties[SpectralProfileKeys.SpectralProfile]);
        Assert.That(decoded!.LengthInSeconds, Is.EqualTo(2));
    }

    [Test]
    public void ShouldTrimOverlapAndConcatRemainder()
    {
        // overlap case: right starts 1s into left → 1s overlap. Merged duration = 3s, so the profile must be 3 seconds.
        // earlier (left) contributes 2 seconds; later (right) drops its first 1s (the overlap) and contributes 1 second.
        var leftProfile = new SpectralProfile(new List<SpectralSecond> { new (0.10, 0.20), new (0.20, 0.30) });
        var rightProfile = new SpectralProfile(new List<SpectralSecond> { new (0.50, 0.60), new (0.60, 0.70) });
        var leftPayload = SpectralProfileCodecRegistry.Default.Encode(leftProfile);
        var rightPayload = SpectralProfileCodecRegistry.Default.Encode(rightProfile);

        var t = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var left = MakeHashes(t, durationSeconds: 2.0, profilePayload: leftPayload);
        var right = MakeHashes(t.AddSeconds(1), durationSeconds: 2.0, profilePayload: rightPayload);

        var merged = left.MergeWith(right);
        var decoded = SpectralProfileCodecRegistry.Default.Decode(merged.Properties[SpectralProfileKeys.SpectralProfile]);

        Assert.That(decoded, Is.Not.Null);
        Assert.That(decoded!.LengthInSeconds, Is.EqualTo(3));
        // earlier side (left) keeps both of its seconds
        Assert.That(decoded.PerSecond[0].Sfm, Is.EqualTo(0.10).Within(0.01));
        Assert.That(decoded.PerSecond[1].Sfm, Is.EqualTo(0.20).Within(0.01));
        // later side (right) dropped its first second (overlap with left's tail); only its second-second survives
        Assert.That(decoded.PerSecond[2].Sfm, Is.EqualTo(0.60).Within(0.01));
    }

    [Test]
    public void OverlapTrimShouldBeOrderIndependentOnRelativeTo()
    {
        // calling Merge(a, b) vs Merge(b, a) must produce equivalent profiles: the "later" side is the one with the
        // larger RelativeTo, regardless of which side gets passed as left/right to MergeWith.
        var earlierProfile = new SpectralProfile(new List<SpectralSecond> { new (0.10, 0.20), new (0.20, 0.30) });
        var laterProfile = new SpectralProfile(new List<SpectralSecond> { new (0.50, 0.60), new (0.60, 0.70) });
        var earlierPayload = SpectralProfileCodecRegistry.Default.Encode(earlierProfile);
        var laterPayload = SpectralProfileCodecRegistry.Default.Encode(laterProfile);

        var tEarlier = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var tLater = tEarlier.AddSeconds(1);
        var earlier = MakeHashes(tEarlier, durationSeconds: 2.0, profilePayload: earlierPayload);
        var later = MakeHashes(tLater, durationSeconds: 2.0, profilePayload: laterPayload);

        var mergedAB = earlier.MergeWith(later).Properties[SpectralProfileKeys.SpectralProfile];
        var mergedBA = later.MergeWith(earlier).Properties[SpectralProfileKeys.SpectralProfile];

        Assert.That(mergedBA, Is.EqualTo(mergedAB));
    }

    [Test]
    public void ShouldConcatProfilesWhenGapIsWithinFingerprintDriftTolerance()
    {
        // gap_seconds = 0.3 ≤ 0.5 contiguity tolerance → still concatenates as if contiguous
        var leftProfile = new SpectralProfile(new List<SpectralSecond> { new (0.10, 0.20) });
        var rightProfile = new SpectralProfile(new List<SpectralSecond> { new (0.50, 0.60) });
        var leftPayload = SpectralProfileCodecRegistry.Default.Encode(leftProfile);
        var rightPayload = SpectralProfileCodecRegistry.Default.Encode(rightProfile);

        var t = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var left = MakeHashes(t, durationSeconds: 1.0, profilePayload: leftPayload);
        // right starts 1.3s after t → 0.3s gap after left's 1s duration; under the 0.5s tolerance.
        var right = MakeHashes(t.AddSeconds(1.3), durationSeconds: 1.0, profilePayload: rightPayload);

        var merged = left.MergeWith(right);

        Assert.That(merged.Properties.ContainsKey(SpectralProfileKeys.SpectralProfile), Is.True);
        var decoded = SpectralProfileCodecRegistry.Default.Decode(merged.Properties[SpectralProfileKeys.SpectralProfile]);
        Assert.That(decoded!.LengthInSeconds, Is.EqualTo(2), "sub-tolerance gap is treated as contiguous — no synthetic fill, no drop");
    }

    [Test]
    public void ShouldDropProfileWhenGapExceedsTolerance()
    {
        // gap_seconds = 2.0 > 0.5 tolerance → real silent region, drop profile rather than invent missing seconds
        var leftProfile = new SpectralProfile(new List<SpectralSecond> { new (0.10, 0.20) });
        var rightProfile = new SpectralProfile(new List<SpectralSecond> { new (0.50, 0.60) });
        var leftPayload = SpectralProfileCodecRegistry.Default.Encode(leftProfile);
        var rightPayload = SpectralProfileCodecRegistry.Default.Encode(rightProfile);

        var t = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var left = MakeHashes(t, durationSeconds: 1.0, profilePayload: leftPayload);
        // right starts 3s after t → 2s silent gap after left's 1s duration; well past tolerance.
        var right = MakeHashes(t.AddSeconds(3.0), durationSeconds: 1.0, profilePayload: rightPayload);

        var merged = left.MergeWith(right);

        Assert.That(merged.Properties.ContainsKey(SpectralProfileKeys.SpectralProfile), Is.False);
    }

    [Test]
    public void ShouldDropProfileKeyWhenOnlyOneSideCarriesIt()
    {
        var leftProfile = new SpectralProfile(new List<SpectralSecond> { new (0.10, 0.20) });
        var leftPayload = SpectralProfileCodecRegistry.Default.Encode(leftProfile);

        var t = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var left = MakeHashes(t, durationSeconds: 1.0, profilePayload: leftPayload);
        var right = MakeHashes(t.AddSeconds(1), durationSeconds: 1.0, profilePayload: null);

        var merged = left.MergeWith(right);

        Assert.That(merged.Properties.ContainsKey(SpectralProfileKeys.SpectralProfile), Is.False);
    }

    private static Hashes MakeHashes(DateTime relativeTo, double durationSeconds, string? profilePayload)
    {
        var hashedFingerprint = new HashedFingerprint(new[] { 1, 2, 3 }, 0, 0, Array.Empty<byte>());
        var properties = new Dictionary<string, string>();
        if (profilePayload != null)
        {
            properties[SpectralProfileKeys.SpectralProfile] = profilePayload;
        }

        return new Hashes(new[] { hashedFingerprint }, durationSeconds, MediaType.Audio, relativeTo, new[] { "origin" }, "stream", properties, 0);
    }
}
