namespace SoundFingerprinting.SFM;

using System;
using System.Collections.Generic;
using System.Linq;
using SoundFingerprinting.Query;

/// <summary>
///  Bridges per-second regions whose spectral character is close on both sides, with a tight tolerance
///  and a hard cumulative cap to bound false-positive risk for infomercial-style variable tails.
/// </summary>
/// <remarks>
///  Per-second emit iff <c>|Q.sfm[i] - T.sfm[i]| &lt; Tolerance</c>. Phase 2 truncates the emissions at
///  <c>min(MaxBridgedSecondsAbsolute, MaxBridgedSecondsRelative × queryLength)</c> (exposed via
///  <see cref="MaxAbsoluteBridgeSeconds"/> and <see cref="MaxQueryRelativeBridge"/>). The absolute cap is
///  load-bearing safety against false merges on weakly-anchored speech-vs-speech candidates — combined with
///  <c>VerySimilarCoverageThreshold = 0.7</c>, the strategy cannot push such a candidate over the merge
///  threshold by bridging alone.
/// </remarks>
public sealed class SimilarProfileBridgingStrategy : ISfmMatchStrategy
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="SimilarProfileBridgingStrategy"/> class.
    /// </summary>
    /// <param name="tolerance">
    ///  Maximum allowed |ΔSFM| between query and track at the same second. Default 0.10 — tight default;
    ///  both sides must be in roughly the same spectral band. Valid range [0.05, 0.20].
    /// </param>
    /// <param name="maxBridgedSecondsAbsolute">
    ///  Absolute cumulative cap (seconds). Default 10. Valid range [5, 20].
    /// </param>
    /// <param name="maxBridgedSecondsRelative">
    ///  Relative cumulative cap as a fraction of queryLength. Default 0.30. Valid range [0.20, 0.50].
    /// </param>
    public SimilarProfileBridgingStrategy(double tolerance = 0.10, double maxBridgedSecondsAbsolute = 10, double maxBridgedSecondsRelative = 0.30)
    {
        if (tolerance is < 0.05 or > 0.20)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Must be in [0.05, 0.20].");
        }

        if (maxBridgedSecondsAbsolute is < 5 or > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBridgedSecondsAbsolute), maxBridgedSecondsAbsolute, "Must be in [5, 20].");
        }

        if (maxBridgedSecondsRelative is < 0.20 or > 0.50)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBridgedSecondsRelative), maxBridgedSecondsRelative, "Must be in [0.20, 0.50].");
        }

        Tolerance = tolerance;
        MaxBridgedSecondsAbsolute = maxBridgedSecondsAbsolute;
        MaxBridgedSecondsRelative = maxBridgedSecondsRelative;
    }

    /// <summary>
    ///  Gets the singleton instance with calibrated defaults.
    /// </summary>
    public static SimilarProfileBridgingStrategy Default { get; } = new ();

    /// <summary>
    ///  Gets the per-second |ΔSFM| tolerance.
    /// </summary>
    public double Tolerance { get; }

    /// <summary>
    ///  Gets the absolute cap on cumulative bridged seconds per candidate.
    /// </summary>
    public double MaxBridgedSecondsAbsolute { get; }

    /// <summary>
    ///  Gets the relative cap on cumulative bridged seconds per candidate, expressed as a fraction of queryLength.
    /// </summary>
    public double MaxBridgedSecondsRelative { get; }

    /// <inheritdoc />
    /// <remarks>For this strategy the query-relative bridge cap is <see cref="MaxBridgedSecondsRelative"/>.</remarks>
    public double MaxQueryRelativeBridge => MaxBridgedSecondsRelative;

    /// <inheritdoc />
    /// <remarks>The calibrated absolute cap is <see cref="MaxBridgedSecondsAbsolute"/>.</remarks>
    public double MaxAbsoluteBridgeSeconds => MaxBridgedSecondsAbsolute;

    /// <inheritdoc />
    public IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context)
    {
        if (context.RealMatches.Count == 0)
        {
            return [];
        }

        // no self-truncation here: Phase 2 enforces min(MaxAbsoluteBridgeSeconds, MaxQueryRelativeBridge × queryLength)
        // uniformly for every strategy, counting only synthetics that survive skip-if-real.
        return SyntheticCandidateUtils.EmitPerSecondCandidates(
            context.QueryProfile,
            context.TrackProfile,
            context.QueryLength,
            context.RealMatches,
            (query, track, queryIndex, trackIndex) => Math.Abs(query.PerSecond[queryIndex].Sfm - track.PerSecond[trackIndex].Sfm) < Tolerance,
            MatchedWithType.SimilarProfile);
    }
}