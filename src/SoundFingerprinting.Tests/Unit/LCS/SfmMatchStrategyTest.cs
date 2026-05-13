namespace SoundFingerprinting.Tests.Unit.LCS;

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.Audio;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;
using SoundFingerprinting.SFM;

[TestFixture]
public class SfmMatchStrategyTest
{
    [Test]
    public void NoBridgingStrategyShouldEmitNothing()
    {
        var context = MakeContext(realQueryMatchAts: new[] { 0f, 1f }, querySfms: new[] { 0.5, 0.5 }, trackSfms: new[] { 0.5, 0.5 });

        Assert.That(NoBridgingStrategy.Default.GenerateCandidates(context).ToList(), Is.Empty);
    }

    [Test]
    public void BroadbandShouldEmitWhenBothSidesAboveThresholdAndClose()
    {
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f, 1f },
            querySfms: new[] { 0.85, 0.80, 0.85, 0.30 },
            trackSfms: new[] { 0.85, 0.80, 0.85, 0.30 });

        var candidates = BroadbandNoiseBridgingStrategy.Default.GenerateCandidates(context).ToList();

        // seconds 0, 1, 2 pass (both > 0.70, |Δ| < 0.15); second 3 fails (both 0.30 < 0.70)
        Assert.That(candidates, Has.Count.EqualTo(3));
    }

    [Test]
    public void BroadbandShouldNotEmitWhenOneSideIsClean()
    {
        // Q broadband, T clean speech — should refuse
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: new[] { 0.85, 0.85 },
            trackSfms: new[] { 0.30, 0.30 });

        Assert.That(BroadbandNoiseBridgingStrategy.Default.GenerateCandidates(context).ToList(), Is.Empty);
    }

    [Test]
    public void BroadbandShouldRefuseWhenNoRealAnchors()
    {
        var context = MakeContext(
            realQueryMatchAts: System.Array.Empty<float>(),
            querySfms: new[] { 0.85, 0.85 },
            trackSfms: new[] { 0.85, 0.85 });

        Assert.That(BroadbandNoiseBridgingStrategy.Default.GenerateCandidates(context).ToList(), Is.Empty);
    }

    [Test]
    public void SilentRegionShouldEmitWhenBothSidesQuiet()
    {
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: new[] { 0.5, 0.5, 0.5 },
            trackSfms: new[] { 0.5, 0.5, 0.5 },
            queryPowers: new[] { 1.0, 0.02, 0.03 },
            trackPowers: new[] { 1.0, 0.02, 0.03 });

        var candidates = SilentRegionBridgingStrategy.Default.GenerateCandidates(context).ToList();

        // second 0 fails (power = 1.0); seconds 1 and 2 pass (both < 0.05, |Δ| < 0.05)
        Assert.That(candidates, Has.Count.EqualTo(2));
    }

    [Test]
    public void SimilarProfileShouldRespectAbsoluteCap()
    {
        // 20s ad — relative cap = 6s, absolute cap = 10s → min = 6s
        var qSfms = Enumerable.Repeat(0.30, 20).ToArray();
        var tSfms = Enumerable.Repeat(0.30, 20).ToArray();
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: qSfms,
            trackSfms: tSfms,
            queryLength: 20);

        var candidates = SimilarProfileBridgingStrategy.Default.GenerateCandidates(context).ToList();

        Assert.That(candidates, Has.Count.EqualTo(6));
    }

    [Test]
    public void SimilarProfileShouldRespectAbsoluteCapAtLargeQueryLength()
    {
        // 60s ad — relative cap = 18s, absolute cap = 10s → min = 10s
        var qSfms = Enumerable.Repeat(0.30, 60).ToArray();
        var tSfms = Enumerable.Repeat(0.30, 60).ToArray();
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: qSfms,
            trackSfms: tSfms,
            queryLength: 60);

        var candidates = SimilarProfileBridgingStrategy.Default.GenerateCandidates(context).ToList();

        Assert.That(candidates, Has.Count.EqualTo(10));
    }

    [Test]
    public void CompositeShouldEmitUnionOfInnerStrategies()
    {
        // sec 1: silent (P<5% both sides) → Silent emits, Broadband doesn't (SFM=0.30)
        // sec 2: broadband (SFM>0.70 both sides) → Broadband emits, Silent doesn't (P=0.50)
        // sec 3: neither → nothing emits
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: new[] { 0.5, 0.30, 0.85, 0.30 },
            trackSfms: new[] { 0.5, 0.30, 0.85, 0.30 },
            queryPowers: new[] { 1.0, 0.02, 0.50, 0.50 },
            trackPowers: new[] { 1.0, 0.02, 0.50, 0.50 });

        var composite = new CompositeBridgingStrategy(SilentRegionBridgingStrategy.Default, BroadbandNoiseBridgingStrategy.Default);
        var candidates = composite.GenerateCandidates(context).ToList();

        Assert.That(candidates.Count, Is.EqualTo(2));
        Assert.That(candidates.Select(c => c.QueryMatchAt), Is.EquivalentTo(new[] { 1.0, 2.0 }));
    }

    [Test]
    public void CompositeShouldDeduplicateOverlappingEmissionsAtSameSecond()
    {
        // sec 1: borderline frame where SFM=0.85 (broadband) AND P=0.02 (silent) — both fire
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: new[] { 0.5, 0.85 },
            trackSfms: new[] { 0.5, 0.85 },
            queryPowers: new[] { 1.0, 0.02 },
            trackPowers: new[] { 1.0, 0.02 });

        var composite = new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, SilentRegionBridgingStrategy.Default);
        var candidates = composite.GenerateCandidates(context).ToList();

        // both inner strategies emit sec 1 — composite dedups to a single candidate
        Assert.That(candidates.Count, Is.EqualTo(1));
        Assert.That(candidates[0].QueryMatchAt, Is.EqualTo(1.0));
    }

    [Test]
    public void CompositeShouldExposeBroadbandOrSilentSingleton()
    {
        Assert.That(CompositeBridgingStrategy.BroadbandOrSilent.InnerStrategies.Count, Is.EqualTo(2));
        Assert.That(CompositeBridgingStrategy.BroadbandOrSilent.InnerStrategies, Has.Some.InstanceOf<BroadbandNoiseBridgingStrategy>());
        Assert.That(CompositeBridgingStrategy.BroadbandOrSilent.InnerStrategies, Has.Some.InstanceOf<SilentRegionBridgingStrategy>());
    }

    [Test]
    public void CompositeShouldRejectSimilarProfileStrategy()
    {
        Assert.That(
            () => new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, SimilarProfileBridgingStrategy.Default),
            Throws.ArgumentException);
    }

    [Test]
    public void CompositeShouldRejectNoBridgingStrategy()
    {
        Assert.That(
            () => new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, NoBridgingStrategy.Default),
            Throws.ArgumentException);
    }

    [Test]
    public void CompositeShouldRejectNestedComposite()
    {
        Assert.That(
            () => new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, CompositeBridgingStrategy.BroadbandOrSilent),
            Throws.ArgumentException);
    }

    [Test]
    public void CompositeShouldRejectFewerThanTwoStrategies()
    {
        Assert.That(() => new CompositeBridgingStrategy(), Throws.ArgumentException);
        Assert.That(() => new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default), Throws.ArgumentException);
    }

    private static SyntheticMatchContext MakeContext(
        float[] realQueryMatchAts,
        double[] querySfms,
        double[] trackSfms,
        double[]? queryPowers = null,
        double[]? trackPowers = null,
        double? queryLength = null)
    {
        var qPowers = queryPowers ?? Enumerable.Repeat(0.5, querySfms.Length).ToArray();
        var tPowers = trackPowers ?? Enumerable.Repeat(0.5, trackSfms.Length).ToArray();
        var qProfile = new SpectralProfile(querySfms.Zip(qPowers, (s, p) => new SpectralSecond(s, p)).ToList());
        var tProfile = new SpectralProfile(trackSfms.Zip(tPowers, (s, p) => new SpectralSecond(s, p)).ToList());
        var reals = realQueryMatchAts.Select((at, i) => new MatchedWith((uint)i, at, (uint)i, at, score: 1)).ToList();
        return new SyntheticMatchContext(reals, qProfile, tProfile, queryLength ?? querySfms.Length, trackSfms.Length, fingerprintLength: 1.0);
    }
}
