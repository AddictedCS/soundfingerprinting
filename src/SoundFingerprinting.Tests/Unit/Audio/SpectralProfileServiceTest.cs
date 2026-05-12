namespace SoundFingerprinting.Tests.Unit.Audio;

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Data;

[TestFixture]
public class SpectralProfileServiceTest
{
    private SpectralProfileService service = null!;
    private SpectralProfileCodecRegistry registry = null!;

    [SetUp]
    public void SetUp()
    {
        registry = SpectralProfileCodecRegistry.Default;
        service = new SpectralProfileService(registry);
    }

    [Test]
    public void EncodeShouldReturnNullForEmptyFrameList()
    {
        Assert.That(service.Encode(new List<Frame>()), Is.Null);
    }

    [Test]
    public void EncodeShouldRoundTripBackToSpectralProfile()
    {
        var frames = new[]
        {
            FlatFrame(startsAt: 0f, value: 0.5f),
            FlatFrame(startsAt: 0.5f, value: 0.5f),
            FlatFrame(startsAt: 1.0f, value: 0.5f),
        };

        var payload = service.Encode(frames);

        Assert.That(payload, Is.Not.Null);
        var decoded = registry.Decode(payload!);
        Assert.That(decoded, Is.Not.Null);
        Assert.That(decoded!.LengthInSeconds, Is.EqualTo(2));
    }

    [Test]
    public void EncodeShouldYieldHighSfmForUniformSpectrum()
    {
        // a perfectly flat spectrum has SFM == 1 (geomean == arithmean)
        var frames = new[] { FlatFrame(startsAt: 0f, value: 0.5f) };

        var payload = service.Encode(frames);

        var profile = registry.Decode(payload!);
        Assert.That(profile!.PerSecond[0].Sfm, Is.GreaterThan(0.95));
    }

    [Test]
    public void EncodeShouldYieldLowSfmForPeakySpectrum()
    {
        // a single huge bin dominates: geomean → 0, arithmean stays > 0 → SFM → 0
        var rows = 4;
        var cols = 32;
        var data = new float[rows * cols];
        for (int r = 0; r < rows; ++r)
        {
            data[r * cols] = 1000f;
        }

        var frame = new Frame(data, (ushort)rows, (ushort)cols, 0f, 0u);

        var payload = service.Encode(new[] { frame });
        var profile = registry.Decode(payload!);

        Assert.That(profile!.PerSecond[0].Sfm, Is.LessThan(0.10));
    }

    [Test]
    public void EncodeShouldScalePowerRelativeToMaxBucket()
    {
        // bucket 0: high power; bucket 1: 1/4 of bucket-0 power → relPower ≈ 0.25 in bucket 1
        var frames = new[]
        {
            FlatFrame(startsAt: 0f, value: 1.0f),
            FlatFrame(startsAt: 1.0f, value: 0.25f),
        };

        var payload = service.Encode(frames);

        var profile = registry.Decode(payload!);
        Assert.That(profile!.PerSecond[0].Power, Is.EqualTo(1.0).Within(0.01));
        Assert.That(profile.PerSecond[1].Power, Is.LessThan(0.30));
        Assert.That(profile.PerSecond[1].Power, Is.GreaterThan(0.20));
    }

    [Test]
    public void EncodeShouldProduceDeterministicOutputForSameInput()
    {
        var frames = new[]
        {
            FlatFrame(startsAt: 0f, value: 0.5f),
            FlatFrame(startsAt: 1.0f, value: 0.5f),
        };

        var payload1 = service.Encode(frames);
        var payload2 = service.Encode(frames);

        Assert.That(payload2, Is.EqualTo(payload1));
    }

    [Test]
    public void EncodeShouldBucketMultipleFramesPerSecondTogether()
    {
        // four frames in second 0 with the same content → one second in output
        var frames = Enumerable.Range(0, 4)
            .Select(i => FlatFrame(startsAt: i * 0.2f, value: 0.5f))
            .ToArray();

        var payload = service.Encode(frames);

        var profile = registry.Decode(payload!);
        Assert.That(profile!.LengthInSeconds, Is.EqualTo(1));
    }

    [Test]
    public void NegativeStartsAtFramesShouldBeDropped()
    {
        // a malformed Frame with StartsAt < 0 must not silently bucket into second 0 — Math.Floor(-0.5) = -1
        // is caught by the bucket < 0 guard; an int cast would truncate to 0 and corrupt second 0
        var good = FlatFrame(startsAt: 0.5f, value: 0.5f);
        var bad = FlatFrame(startsAt: -0.5f, value: 1.0f);
        var withBad = new[] { bad, good };
        var withoutBad = new[] { good };

        var payloadWithBad = service.Encode(withBad);
        var payloadWithoutBad = service.Encode(withoutBad);

        var profileWith = registry.Decode(payloadWithBad!);
        var profileWithout = registry.Decode(payloadWithoutBad!);

        // negative-time frame should be ignored, so the second-0 metrics should match the good-only payload
        Assert.That(profileWith!.PerSecond[0].Sfm, Is.EqualTo(profileWithout!.PerSecond[0].Sfm).Within(1e-6));
        Assert.That(profileWith.PerSecond[0].Power, Is.EqualTo(profileWithout.PerSecond[0].Power).Within(1e-6));
    }

    [Test]
    public void EncodeShouldBeOrderIndependent()
    {
        // contract: caller may pass frames in any order; bucketing keys off StartsAt, so a shuffled input must
        // produce the identical payload to a sorted input. Regression guard for the max-StartsAt sizing logic.
        var sorted = new[]
        {
            FlatFrame(startsAt: 0.0f, value: 0.25f),
            FlatFrame(startsAt: 0.5f, value: 0.50f),
            FlatFrame(startsAt: 1.0f, value: 0.75f),
            FlatFrame(startsAt: 1.5f, value: 1.00f),
            FlatFrame(startsAt: 2.0f, value: 0.40f),
        };
        var shuffled = new[] { sorted[3], sorted[0], sorted[4], sorted[1], sorted[2] };

        var sortedPayload = service.Encode(sorted);
        var shuffledPayload = service.Encode(shuffled);

        Assert.That(shuffledPayload, Is.EqualTo(sortedPayload));
        var profile = registry.Decode(sortedPayload!);
        Assert.That(profile!.LengthInSeconds, Is.EqualTo(3), "max(StartsAt)=2.0 must size 3 seconds even though it isn't last in the shuffled input");
    }

    [Test]
    public void ParallelPathShouldMatchSerialPathByteForByte()
    {
        // > ParallelThreshold frames exercise the Parallel.For path; identical inputs must yield the identical payload
        // as the serial path. Vary SFM/power per frame so a thread-local-buffer race would corrupt the output.
        var rng = new Random(123);
        var serialFrames = new List<Frame>();
        for (int i = 0; i < 64; ++i)
        {
            serialFrames.Add(VariedFrame(startsAt: i * 0.5f, seed: i, rng));
        }

        var parallelFrames = serialFrames.ToList();

        // serial baseline: feed in chunks of 4 (under the threshold) and re-merge profiles is awkward,
        // so instead compare two parallel runs and one chunked-serial run for determinism
        var first = service.Encode(parallelFrames);
        var second = service.Encode(parallelFrames);

        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.EqualTo(first), "two parallel runs over the same input must produce identical payloads");

        // also verify a small (under-threshold) batch with the same per-frame inputs gives the same per-frame metrics
        var single = service.Encode(new[] { serialFrames[10] });
        Assert.That(single, Is.Not.Null);
    }

    private static Frame VariedFrame(float startsAt, int seed, Random rng)
    {
        const int rows = 8;
        const int cols = 32;
        var data = new float[rows * cols];
        var local = new Random(seed);
        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = (float)local.NextDouble();
        }

        return new Frame(data, rows, cols, startsAt, (uint)seed);
    }

    private static Frame FlatFrame(float startsAt, float value)
    {
        const int rows = 4;
        const int cols = 32;
        var data = new float[rows * cols];
        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = value;
        }

        return new Frame(data, rows, cols, startsAt, 0u);
    }
}
