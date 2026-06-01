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

        // seconds 0, 1, 2 pass (both > 0.40 floor, windowed |Δ| < 0.15); second 3 fails (both 0.30 < 0.40)
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
    public void SimilarProfileGenerateCandidatesShouldNoLongerSelfCap()
    {
        // the cap moved to Phase 2 — GenerateCandidates now emits every gate-passing second (here all 20),
        // unbounded by the 10s/0.30 cap that used to live inside it (see GetCoveragesWithBridgingTest for Phase-2 enforcement).
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: Enumerable.Repeat(0.30, 20).ToArray(),
            trackSfms: Enumerable.Repeat(0.30, 20).ToArray(),
            queryLength: 20);

        var candidates = SimilarProfileBridgingStrategy.Default.GenerateCandidates(context).ToList();

        Assert.That(candidates, Has.Count.EqualTo(20));
    }

    [Test]
    public void CompositeShouldEmitUnionOfInnerStrategies()
    {
        // sec 0: neither (SFM 0.30 < floor, loud) → nothing
        // sec 1: silent (P<5% both sides) → Silent emits, Broadband doesn't (SFM=0.30)
        // sec 2: broadband (SFM > floor both sides) → Broadband emits, Silent doesn't (P=0.50)
        // sec 3: neither → nothing emits
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: new[] { 0.30, 0.30, 0.85, 0.30 },
            trackSfms: new[] { 0.30, 0.30, 0.85, 0.30 },
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
        // sec 0: neither (SFM 0.30 < floor, loud). sec 1: borderline frame where SFM=0.85 (broadband) AND P=0.02 (silent) — both fire
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f },
            querySfms: new[] { 0.30, 0.85 },
            trackSfms: new[] { 0.30, 0.85 },
            queryPowers: new[] { 1.0, 0.02 },
            trackPowers: new[] { 1.0, 0.02 });

        var composite = new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, SilentRegionBridgingStrategy.Default);
        var candidates = composite.GenerateCandidates(context).ToList();

        // both inner strategies emit sec 1 — composite dedups to a single candidate
        Assert.That(candidates.Count, Is.EqualTo(1));
        Assert.That(candidates[0].QueryMatchAt, Is.EqualTo(1.0));
    }

    [Test]
    public void CompositeMayIncludeSimilarProfileAndInheritsItsTightCap()
    {
        // SimilarProfile is now composable: its absolute cap travels via MaxAbsoluteBridgeSeconds and is preserved
        // under the min combine, so the composite inherits its tight min(10s, 0.30 x queryLength) bound.
        var composite = new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, SimilarProfileBridgingStrategy.Default);

        Assert.That(composite.MaxAbsoluteBridgeSeconds, Is.EqualTo(10), "min(inf, 10)");
        Assert.That(composite.MaxQueryRelativeBridge, Is.EqualTo(0.30), "min(0.70, 0.30)");
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
            () => new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, 
                new CompositeBridgingStrategy(SilentRegionBridgingStrategy.Default, BroadbandNoiseBridgingStrategy.Default)),
            Throws.ArgumentException);
    }

    [Test]
    public void CompositeShouldRejectFewerThanTwoStrategies()
    {
        Assert.That(() => new CompositeBridgingStrategy(), Throws.ArgumentException);
        Assert.That(() => new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default), Throws.ArgumentException);
    }

    [Test]
    public void StrategiesShouldExposeDefaultMaxQueryRelativeBridge()
    {
        Assert.That(BroadbandNoiseBridgingStrategy.Default.MaxQueryRelativeBridge, Is.EqualTo(0.70));
        Assert.That(SilentRegionBridgingStrategy.Default.MaxQueryRelativeBridge, Is.EqualTo(0.70));
        Assert.That(SimilarProfileBridgingStrategy.Default.MaxQueryRelativeBridge, Is.EqualTo(0.30), "SimilarProfile's relative cap is its query-relative bridge");
        Assert.That(NoBridgingStrategy.Default.MaxQueryRelativeBridge, Is.EqualTo(0));
    }

    [Test]
    public void StrategiesShouldExposeDefaultMaxAbsoluteBridgeSeconds()
    {
        Assert.That(BroadbandNoiseBridgingStrategy.Default.MaxAbsoluteBridgeSeconds, Is.EqualTo(double.PositiveInfinity));
        Assert.That(SilentRegionBridgingStrategy.Default.MaxAbsoluteBridgeSeconds, Is.EqualTo(double.PositiveInfinity));
        Assert.That(NoBridgingStrategy.Default.MaxAbsoluteBridgeSeconds, Is.EqualTo(double.PositiveInfinity));
        Assert.That(new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, SilentRegionBridgingStrategy.Default).MaxAbsoluteBridgeSeconds, Is.EqualTo(double.PositiveInfinity), "min(inf, inf)");
        Assert.That(SimilarProfileBridgingStrategy.Default.MaxAbsoluteBridgeSeconds, Is.EqualTo(10));
    }

    [Test]
    public void StrategiesShouldExposeOverriddenMaxQueryRelativeBridge()
    {
        Assert.That(new BroadbandNoiseBridgingStrategy(maxQueryRelativeBridge: 0.9).MaxQueryRelativeBridge, Is.EqualTo(0.9));
        Assert.That(new SilentRegionBridgingStrategy(maxQueryRelativeBridge: 0.5).MaxQueryRelativeBridge, Is.EqualTo(0.5));
    }

    [Test]
    public void CompositeShouldDeriveMaxQueryRelativeBridgeAsMinOfInnerLegs()
    {
        // the strictest leg governs the merged union — no gate's synthetics can exceed its own declared ceiling
        var composite = new CompositeBridgingStrategy(
            new BroadbandNoiseBridgingStrategy(maxQueryRelativeBridge: 0.9),
            new SilentRegionBridgingStrategy(maxQueryRelativeBridge: 0.5));
        Assert.That(composite.MaxQueryRelativeBridge, Is.EqualTo(0.5));
    }

    [Test]
    public void MaxQueryRelativeBridgeOutOfRangeShouldThrow()
    {
        Assert.That(() => new BroadbandNoiseBridgingStrategy(maxQueryRelativeBridge: 1.1), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        Assert.That(() => new SilentRegionBridgingStrategy(maxQueryRelativeBridge: -0.1), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
    }

    [Test]
    public void BroadbandShouldAllowSubFiftySfmThresholdForWindowedUse()
    {
        Assert.That(() => new BroadbandNoiseBridgingStrategy(sfmThreshold: 0.40), Throws.Nothing);
        Assert.That(() => new BroadbandNoiseBridgingStrategy(sfmThreshold: 0.19), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        Assert.That(() => new BroadbandNoiseBridgingStrategy(agreementWindowRadius: -1), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        Assert.That(new BroadbandNoiseBridgingStrategy(agreementWindowRadius: 2).AgreementWindowRadius, Is.EqualTo(2));
    }

    [Test]
    public void WindowedBroadbandShouldRejectCoincidentalSingleSecondAgreementThatPointwiseAccepts()
    {
        // a long broadband gap (all SFM > 0.40) where track disagrees with query (|ΔSFM| ~0.30) everywhere EXCEPT two
        // isolated coincidental-agreement spikes (q=4, q=8). pointwise accepts the two lucky seconds; the windowed mean
        // around each is dominated by the surrounding disagreement and rejects them — the splice-style discrimination.
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f, 1f },
            querySfms: new[] { 0.5, 0.5, 0.85, 0.85, 0.85, 0.85, 0.85, 0.85, 0.85, 0.85, 0.85 },
            trackSfms: new[] { 0.5, 0.5, 0.55, 0.55, 0.85, 0.55, 0.55, 0.55, 0.85, 0.55, 0.55 });

        int pointwise = new BroadbandNoiseBridgingStrategy(sfmThreshold: 0.40, agreementWindowRadius: 0).GenerateCandidates(context).Count();
        int windowed = new BroadbandNoiseBridgingStrategy(sfmThreshold: 0.40, agreementWindowRadius: 2).GenerateCandidates(context).Count();

        Assert.That(pointwise, Is.GreaterThan(windowed), "pointwise accepts the coincidental agreement spikes; the window rejects them as the neighbourhood disagrees");
    }

    [Test]
    public void WindowedBroadbandShouldStillBridgeGenuineAgreementSmoothingPerSecondNoise()
    {
        // genuine same-content gap: query/track agree on average but jitter per-second by up to ~0.18 (a couple of
        // seconds individually exceed the 0.15 pointwise tolerance). the windowed mean stays under tolerance, so the
        // window bridges MORE than pointwise — smoothing legitimate per-second SFM noise.
        var context = MakeContext(
            realQueryMatchAts: new[] { 0f, 1f },
            querySfms: new[] { 0.5, 0.5, 0.80, 0.80, 0.80, 0.80, 0.80 },
            trackSfms: new[] { 0.5, 0.5, 0.80, 0.98, 0.80, 0.62, 0.80 });

        int pointwise = new BroadbandNoiseBridgingStrategy(sfmThreshold: 0.40, agreementWindowRadius: 0).GenerateCandidates(context).Count();
        int windowed = new BroadbandNoiseBridgingStrategy(sfmThreshold: 0.40, agreementWindowRadius: 2).GenerateCandidates(context).Count();

        Assert.That(windowed, Is.GreaterThan(pointwise), "the window smooths per-second jitter that the pointwise check rejects");
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
