namespace SoundFingerprinting.Tests.Unit.LCS;

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.Audio;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;
using SoundFingerprinting.SFM;

[TestFixture]
public class GetCoveragesWithBridgingTest
{
    private const double FingerprintLength = 1.0;
    private const double PermittedGap = 0;

    [Test]
    public void NoBridgingStrategyShouldNotIntroduceSynthetics()
    {
        var reals = ContiguousMatches(0, 5);

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: 10,
            trackLength: 10,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: NoBridgingStrategy.Default,
            queryProfile: MakeProfile(10, sfm: 0.85, power: 0.5),
            trackProfile: MakeProfile(10, sfm: 0.85, power: 0.5)).ToList();

        Assert.That(coverages, Is.Not.Empty);
        Assert.That(coverages[0].BridgedSeconds, Is.EqualTo(0));
    }

    [Test]
    public void NullProfileShouldShortCircuitToNoBridging()
    {
        var reals = ContiguousMatches(0, 5);

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: 10,
            trackLength: 10,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: BroadbandNoiseBridgingStrategy.Default,
            queryProfile: null,
            trackProfile: MakeProfile(10, sfm: 0.85, power: 0.5)).ToList();

        Assert.That(coverages, Is.Not.Empty);
        Assert.That(coverages[0].BridgedSeconds, Is.EqualTo(0));
    }

    [Test]
    public void LegacyStrategyShouldBypassBridging()
    {
        var reals = ContiguousMatches(0, 5);

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.Legacy,
            queryLength: 10,
            trackLength: 10,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: BroadbandNoiseBridgingStrategy.Default,
            queryProfile: MakeProfile(10, sfm: 0.85, power: 0.5),
            trackProfile: MakeProfile(10, sfm: 0.85, power: 0.5)).ToList();

        Assert.That(coverages, Is.Not.Empty);
        Assert.That(coverages[0].BridgedSeconds, Is.EqualTo(0));
    }

    [Test]
    public void BroadbandBridgingShouldExtendCoverageIntoTail()
    {
        // reals occupy 0-4; tail 5-9 has both sides broadband → bridge expected
        var reals = ContiguousMatches(0, 5);

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: 10,
            trackLength: 10,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: BroadbandNoiseBridgingStrategy.Default,
            queryProfile: MakeProfile(10, sfm: 0.85, power: 0.5),
            trackProfile: MakeProfile(10, sfm: 0.85, power: 0.5)).ToList();

        Assert.That(coverages, Is.Not.Empty);
        Assert.That(coverages[0].BridgedSeconds, Is.GreaterThan(0));
    }

    [Test]
    public void BroadbandBridgingShouldSkipSecondsAlreadyCoveredByReals()
    {
        // reals at 0-4; profile says bridging would also pass at 0-4 — but skip-if-real applies
        var reals = ContiguousMatches(0, 5);

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: 5,
            trackLength: 5,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: BroadbandNoiseBridgingStrategy.Default,
            queryProfile: MakeProfile(5, sfm: 0.85, power: 0.5),
            trackProfile: MakeProfile(5, sfm: 0.85, power: 0.5)).ToList();

        // every second is real-covered → no synthetic should land
        Assert.That(coverages, Is.Not.Empty);
        Assert.That(coverages[0].BridgedSeconds, Is.EqualTo(0));
    }

    [Test]
    public void BridgedPathShouldBeMonotonicOnBothAxesAfterReconstruction()
    {
        // 5 reals at seconds 0-4 (Q,T integer-aligned), broadband everywhere → 5 synthetics in [5, 9]
        var reals = ContiguousMatches(0, 5);

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: 10,
            trackLength: 10,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: BroadbandNoiseBridgingStrategy.Default,
            queryProfile: MakeProfile(10, sfm: 0.85, power: 0.5),
            trackProfile: MakeProfile(10, sfm: 0.85, power: 0.5)).ToList();

        Assert.That(coverages, Is.Not.Empty);
        var bestPath = coverages[0].BestPath.ToList();
        Assert.That(bestPath, Has.Count.GreaterThan(reals.Count));

        // load-bearing LIS invariant: sorting by QueryMatchAt and by QuerySequenceNumber yields the same order
        var byQueryTime = bestPath.OrderBy(m => m.QueryMatchAt).Select(m => m.QuerySequenceNumber).ToList();
        var byQuerySeq = bestPath.OrderBy(m => m.QuerySequenceNumber).Select(m => m.QuerySequenceNumber).ToList();
        Assert.That(byQueryTime, Is.EqualTo(byQuerySeq).AsCollection);

        var byTrackTime = bestPath.OrderBy(m => m.TrackMatchAt).Select(m => m.TrackSequenceNumber).ToList();
        var byTrackSeq = bestPath.OrderBy(m => m.TrackSequenceNumber).Select(m => m.TrackSequenceNumber).ToList();
        Assert.That(byTrackTime, Is.EqualTo(byTrackSeq).AsCollection);
    }

    [Test]
    public void BridgedPathShouldNotReportGapsInBridgedRegion()
    {
        var reals = ContiguousMatches(0, 5);

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: 10,
            trackLength: 10,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: BroadbandNoiseBridgingStrategy.Default,
            queryProfile: MakeProfile(10, sfm: 0.85, power: 0.5),
            trackProfile: MakeProfile(10, sfm: 0.85, power: 0.5)).ToList();

        Assert.That(coverages, Is.Not.Empty);
        var queryGaps = coverages[0].QueryGaps.Where(g => !g.IsOnEdge).ToList();
        Assert.That(queryGaps, Is.Empty, "no interior gaps once the tail is bridged");
    }

    [Test]
    public void BridgedPathShouldStillReportRealGapsWhenNoBridge()
    {
        // reals at 0 and 9; no profile contiguity (silent middle) — without bridging the gap stays visible
        var reals = new List<MatchedWith>
        {
            new (0, 0, 0, 0, score: 1),
            new (9, 9, 9, 9, score: 1),
        };

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: 10,
            trackLength: 10,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: NoBridgingStrategy.Default,
            queryProfile: null,
            trackProfile: null).ToList();

        Assert.That(coverages, Is.Not.Empty);
        var queryGaps = coverages[0].QueryGaps.Where(g => !g.IsOnEdge).ToList();
        Assert.That(queryGaps, Is.Not.Empty, "real gap between 0 and 9 must still be reported");
    }

    [Test]
    public void SimilarProfileShouldNotPushWeaklyAnchoredCandidateOverMergeThreshold()
    {
        // negative case: only 3 reals (anchor) + 25s of bridgeable speech-band content;
        // SimilarProfile's 10s cap means total covered ≤ 13s, well under 0.7 × 28 ≈ 19.6s — no false merge
        var reals = ContiguousMatches(0, 3);
        const double queryLength = 28;
        var profile = MakeProfile((int)queryLength, sfm: 0.30, power: 0.5);

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: queryLength,
            trackLength: queryLength,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: SimilarProfileBridgingStrategy.Default,
            queryProfile: profile,
            trackProfile: profile).ToList();

        Assert.That(coverages, Is.Not.Empty);
        Assert.That(coverages[0].BridgedSeconds, Is.LessThanOrEqualTo(10), "SimilarProfile absolute cap is 10s");
        // anchor (~3s) + bridge (≤10s) = ≤13s. Relative to query of 28s, QueryRelativeCoverage ≤ ~0.47 — stays under 0.7 merge threshold
        Assert.That(coverages[0].QueryRelativeCoverage, Is.LessThan(0.7), "cap prevents weakly-anchored candidate from crossing merge threshold");
    }

    [Test]
    public void BridgedPathBracketInterpolationShouldLandSyntheticInsideBracketSeqRange()
    {
        // brackets: real at Q=10/t=0.5 and Q=110/t=10.5; broadband profile across full duration;
        // synthetic at X=5 should interpolate between Q=10 and Q=110 by time fraction (5-0.5)/(10.5-0.5) = 0.45 → ~55
        var reals = new List<MatchedWith>
        {
            new (10, 0.5f, 10, 0.5f, score: 1),
            new (110, 10.5f, 110, 10.5f, score: 1),
        };

        var coverages = reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: 11,
            trackLength: 11,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: BroadbandNoiseBridgingStrategy.Default,
            queryProfile: MakeProfile(11, sfm: 0.85, power: 0.5),
            trackProfile: MakeProfile(11, sfm: 0.85, power: 0.5)).ToList();

        Assert.That(coverages, Is.Not.Empty);
        var synthetics = coverages[0].BestPath
            .Where(m => m.QuerySequenceNumber > 10 && m.QuerySequenceNumber < 110)
            .ToList();
        Assert.That(synthetics, Is.Not.Empty);
        // every synthetic seq must strictly land between the brackets on both axes
        foreach (var s in synthetics)
        {
            Assert.That(s.QuerySequenceNumber, Is.GreaterThan((uint)10).And.LessThan((uint)110));
            Assert.That(s.TrackSequenceNumber, Is.GreaterThan((uint)10).And.LessThan((uint)110));
        }
    }

    [Test]
    public void CompositeBridgingShouldBeIndependentOfInnerStrategyOrder()
    {
        // Two disjoint gaps that each need a different leg:
        //   head 0-2  : silent (low power, low sfm)  -> only SilentRegion bridges
        //   middle 5-10: broadband (high sfm)        -> only BroadbandNoise bridges
        // reals anchor the two clusters (3,4) and (11,12); tail 13-15 is neither.
        // Before the fix, BroadbandNoise-first emitted the middle run, advanced the Phase-2 forward
        // cursor past all reals, then mis-bracketed the head run as a tail bridge and dropped it —
        // so Composite(BB, Silent) and Composite(Silent, BB) disagreed. They must now be identical.
        var reals = new List<MatchedWith>
        {
            new (3, 3, 3, 3, score: 1),
            new (4, 4, 4, 4, score: 1),
            new (11, 11, 11, 11, score: 1),
            new (12, 12, 12, 12, score: 1),
        };

        var profile = MakeMixedProfile();
        var bbThenSilent = new CompositeBridgingStrategy(BroadbandNoiseBridgingStrategy.Default, SilentRegionBridgingStrategy.Default);
        var silentThenBb = new CompositeBridgingStrategy(SilentRegionBridgingStrategy.Default, BroadbandNoiseBridgingStrategy.Default);

        var a = Bridge(reals, profile, bbThenSilent);
        var b = Bridge(reals, profile, silentThenBb);
        var bbAlone = Bridge(reals, profile, BroadbandNoiseBridgingStrategy.Default);
        var silentAlone = Bridge(reals, profile, SilentRegionBridgingStrategy.Default);

        // commutativity: the two leg orders produce identical bridging
        Assert.That(a.BridgedSeconds, Is.EqualTo(b.BridgedSeconds), "BridgedSeconds must not depend on inner-strategy order");
        Assert.That(a.QueryRelativeCoverage, Is.EqualTo(b.QueryRelativeCoverage).Within(1e-9), "coverage must not depend on inner-strategy order");
        Assert.That(
            a.QueryGaps.Where(g => !g.IsOnEdge).Select(g => (g.Start, g.End)),
            Is.EqualTo(b.QueryGaps.Where(g => !g.IsOnEdge).Select(g => (g.Start, g.End))).AsCollection,
            "interior gaps must not depend on inner-strategy order");

        // correctness: the union genuinely bridges BOTH regions (head via Silent + middle via Broadband),
        // i.e. the composite equals the sum of the disjoint single-strategy contributions — no region dropped.
        Assert.That(a.BridgedSeconds, Is.EqualTo(bbAlone.BridgedSeconds + silentAlone.BridgedSeconds), "composite must bridge the union of both legs' regions");
        Assert.That(a.BridgedSeconds, Is.GreaterThan(bbAlone.BridgedSeconds));
        Assert.That(a.BridgedSeconds, Is.GreaterThan(silentAlone.BridgedSeconds));
    }

    [Test]
    public void RaisingMaxQueryRelativeBridgeShouldBridgePastTheDefaultCap()
    {
        // 2 real anchors at 0,1 + broadband everywhere across a 20s query → 18 bridgeable tail seconds (2-19).
        // default cap 0.70 -> floor(0.70 x 20) = 14 survive; raising to 0.95 -> floor(0.95 x 20) = 19 >= 18 -> all 18.
        var reals = ContiguousMatches(0, 2);
        var profile = MakeProfile(20, sfm: 0.85, power: 0.5);

        var capped = Bridge(reals, profile, new BroadbandNoiseBridgingStrategy(), length: 20);
        var raised = Bridge(reals, profile, new BroadbandNoiseBridgingStrategy(maxQueryRelativeBridge: 0.95), length: 20);

        Assert.That(capped.BridgedSeconds, Is.EqualTo(14), "default cap = floor(0.70 x 20)");
        Assert.That(raised.BridgedSeconds, Is.EqualTo(18), "raised cap lets all 18 bridgeable tail seconds through");
        Assert.That(raised.BridgedSeconds, Is.GreaterThan(capped.BridgedSeconds));
    }

    [Test]
    public void SimilarProfileAbsoluteCapShouldBeEnforcedInPhase2()
    {
        // 60s query, similar profile throughout, 2 anchors -> relative cap = 0.30 x 60 = 18s, absolute = 10s -> min = 10.
        // (The cap now lives in Phase 2, not inside SimilarProfile.GenerateCandidates.)
        var reals = ContiguousMatches(0, 2);
        var profile = MakeProfile(60, sfm: 0.30, power: 0.5);

        var coverage = Bridge(reals, profile, SimilarProfileBridgingStrategy.Default, length: 60);

        Assert.That(coverage.BridgedSeconds, Is.EqualTo(10), "absolute 10s cap binds at 60s query");
    }

    [Test]
    public void SimilarProfileRelativeCapShouldBeEnforcedInPhase2()
    {
        // 20s query -> relative cap = 0.30 x 20 = 6s, absolute = 10s -> min = 6.
        var reals = ContiguousMatches(0, 2);
        var profile = MakeProfile(20, sfm: 0.30, power: 0.5);

        var coverage = Bridge(reals, profile, SimilarProfileBridgingStrategy.Default, length: 20);

        Assert.That(coverage.BridgedSeconds, Is.EqualTo(6), "relative 0.30 cap binds at 20s query");
    }

    private static Coverage Bridge(List<MatchedWith> reals, SpectralProfile profile, ISfmMatchStrategy strategy, double length = 16)
    {
        return reals.GetCoverages(
            QueryPathReconstructionStrategyType.MultipleBestPaths,
            queryLength: length,
            trackLength: length,
            fingerprintLength: FingerprintLength,
            permittedGap: PermittedGap,
            sfmMatchStrategy: strategy,
            queryProfile: profile,
            trackProfile: profile).First();
    }

    // 16s profile: 0-2 silent, 5-10 broadband, everything else neither (reals at 3,4,11,12 are skip-if-real)
    private static SpectralProfile MakeMixedProfile()
    {
        var seconds = new List<SpectralSecond>();
        for (int i = 0; i < 16; i++)
        {
            bool silent = i <= 2;
            bool broadband = i >= 5 && i <= 10;
            double sfm = broadband ? 0.85 : 0.30;
            double power = silent ? 0.02 : 0.50;
            seconds.Add(new SpectralSecond(sfm, power));
        }

        return new SpectralProfile(seconds);
    }

    private static List<MatchedWith> ContiguousMatches(int start, int count)
    {
        return Enumerable.Range(start, count)
            .Select(i => new MatchedWith((uint)i, i, (uint)i, i, score: 1))
            .ToList();
    }

    private static SpectralProfile MakeProfile(int seconds, double sfm, double power)
    {
        return new SpectralProfile(Enumerable.Range(0, seconds).Select(_ => new SpectralSecond(sfm, power)).ToList());
    }
}
