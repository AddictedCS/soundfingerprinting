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
