namespace SoundFingerprinting.Tests.Unit.Audio;

using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoundFingerprinting.Audio;

[TestFixture]
public class SpectralProfileCodecTest
{
    [Test]
    public void V1ShouldRoundTripPerSecondData()
    {
        var profile = new SpectralProfile(new List<SpectralSecond>
        {
            new (0.10, 0.20),
            new (0.55, 0.80),
            new (0.95, 1.00),
        });

        var encoded = SpectralProfileCodecV1.Instance.Encode(profile);
        var decoded = SpectralProfileCodecV1.Instance.Decode(encoded);

        Assert.That(decoded.LengthInSeconds, Is.EqualTo(3));
        // byte quantisation introduces ~1/255 = ~0.004 error per value
        Assert.That(decoded.PerSecond[0].Sfm, Is.EqualTo(0.10).Within(0.01));
        Assert.That(decoded.PerSecond[0].Power, Is.EqualTo(0.20).Within(0.01));
        Assert.That(decoded.PerSecond[1].Sfm, Is.EqualTo(0.55).Within(0.01));
        Assert.That(decoded.PerSecond[2].Power, Is.EqualTo(1.00).Within(0.01));
    }

    [Test]
    public void V1ShouldStartWithVersionByteAndFlagsByte()
    {
        var profile = new SpectralProfile(new List<SpectralSecond> { new (0.5, 0.5) });

        var encoded = SpectralProfileCodecV1.Instance.Encode(profile);

        Assert.That(encoded[0], Is.EqualTo(1));
        Assert.That(encoded[1], Is.EqualTo(0));
        Assert.That(encoded.Length, Is.EqualTo(4));
    }

    [Test]
    public void V1ShouldQuantiseAtSpecBoundaries()
    {
        // 0.70 × 255 = 178 (the BroadbandNoise threshold lands cleanly)
        var profile = new SpectralProfile(new List<SpectralSecond> { new (0.70, 0.05) });

        var encoded = SpectralProfileCodecV1.Instance.Encode(profile);

        Assert.That(encoded[2], Is.EqualTo(178));
        Assert.That(encoded[3], Is.EqualTo((byte)(0.05 * 255)));
    }

    [Test]
    public void RegistryShouldReturnNullForUnknownVersion()
    {
        var registry = SpectralProfileCodecRegistry.Default;

        var unknown = new byte[] { 99, 0, 100, 100 };
        Assert.That(registry.Decode(unknown), Is.Null);
    }

    [Test]
    public void RegistryShouldReturnNullForMalformedBase64()
    {
        var registry = SpectralProfileCodecRegistry.Default;

        Assert.That(registry.Decode("!!!not-base64!!!"), Is.Null);
    }

}
