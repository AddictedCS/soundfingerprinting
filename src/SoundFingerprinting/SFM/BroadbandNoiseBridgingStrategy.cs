namespace SoundFingerprinting.SFM;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///  Bridges per-second regions where both query and track are sustained broadband content
///  (waves, surf, action SFX). Default for ad-deduplication.
/// </summary>
/// <remarks>
///  Per-second emit iff <c>Q.sfm[i] &gt; SfmThreshold AND T.sfm[i] &gt; SfmThreshold AND |Q.sfm[i] - T.sfm[i]| &lt; SimilarityTolerance</c>.
///  No per-strategy cumulative cap — the universal 70% sanity bound in Phase 2 is the only backstop.
/// </remarks>
public sealed class BroadbandNoiseBridgingStrategy : ISfmMatchStrategy
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="BroadbandNoiseBridgingStrategy"/> class.
    /// </summary>
    /// <param name="sfmThreshold">
    ///  Minimum SFM for a second to be considered "broadband" on either side. Default 0.70 — boundary between
    ///  dense music/speech and sustained broadband. Valid range [0.50, 0.90].
    /// </param>
    /// <param name="similarityTolerance">
    ///  Maximum allowed |ΔSFM| between query and track at the same second. Default 0.15 — wider tolerance since
    ///  both sides are already high SFM. Valid range [0.05, 0.30].
    /// </param>
    public BroadbandNoiseBridgingStrategy(double sfmThreshold = 0.70, double similarityTolerance = 0.15)
    {
        if (sfmThreshold < 0.50 || sfmThreshold > 0.90)
        {
            throw new ArgumentOutOfRangeException(nameof(sfmThreshold), sfmThreshold, "Must be in [0.50, 0.90].");
        }

        if (similarityTolerance < 0.05 || similarityTolerance > 0.30)
        {
            throw new ArgumentOutOfRangeException(nameof(similarityTolerance), similarityTolerance, "Must be in [0.05, 0.30].");
        }

        SfmThreshold = sfmThreshold;
        SimilarityTolerance = similarityTolerance;
    }

    /// <summary>
    ///  Gets the singleton instance with calibrated defaults (SfmThreshold=0.70, SimilarityTolerance=0.15).
    /// </summary>
    public static BroadbandNoiseBridgingStrategy Default { get; } = new ();

    /// <summary>
    ///  Gets the per-side SFM threshold.
    /// </summary>
    public double SfmThreshold { get; }

    /// <summary>
    ///  Gets the maximum allowed |ΔSFM| between query and track at the same second.
    /// </summary>
    public double SimilarityTolerance { get; }

    /// <inheritdoc />
    public IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context)
    {
        if (context.RealMatches.Count == 0)
        {
            // structural refusal: whole-track bridge with no anchors is unsafe (no local diagonal to anchor against).
            return Enumerable.Empty<SyntheticCandidate>();
        }

        return SyntheticCandidateUtils.EmitPerSecondCandidates(
            context.QueryProfile,
            context.TrackProfile,
            context.QueryLength,
            context.RealMatches,
            (qSecond, tSecond) =>
                qSecond.Sfm > SfmThreshold &&
                tSecond.Sfm > SfmThreshold &&
                Math.Abs(qSecond.Sfm - tSecond.Sfm) < SimilarityTolerance);
    }
}