namespace SoundFingerprinting.SFM;

using System;
using System.Collections.Generic;
using System.Linq;
using SoundFingerprinting.LCS;

/// <summary>
///  Bridges per-second regions whose spectral character is close on both sides, with a tight tolerance
///  and a hard cumulative cap to bound false-positive risk for infomercial-style variable tails.
/// </summary>
/// <remarks>
///  Per-second emit iff <c>|Q.sfm[i] - T.sfm[i]| &lt; Tolerance</c>. Truncated by a cumulative cap of
///  <c>min(MaxBridgedSecondsAbsolute, MaxBridgedSecondsRelative × queryLength)</c>. The absolute cap is
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
    public IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context)
    {
        if (context.RealMatches.Count == 0)
        {
            return Enumerable.Empty<SyntheticCandidate>();
        }

        double cap = Math.Min(MaxBridgedSecondsAbsolute, MaxBridgedSecondsRelative * context.QueryLength);
        var candidates = SyntheticCandidateUtils.EmitPerSecondCandidates(
            context.QueryProfile,
            context.TrackProfile,
            context.QueryLength,
            context.RealMatches,
            (qSecond, tSecond) => Math.Abs(qSecond.Sfm - tSecond.Sfm) < Tolerance);
        int budget = (int)Math.Floor(cap);
        return candidates.Take(budget);
    }
}