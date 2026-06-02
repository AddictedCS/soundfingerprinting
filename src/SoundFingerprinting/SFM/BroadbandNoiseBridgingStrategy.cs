namespace SoundFingerprinting.SFM;

using System;
using System.Collections.Generic;
using System.Linq;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Query;

/// <summary>
///  Bridges per-second regions where both query and track are sustained broadband content
///  (waves, surf, action SFX). Default for ad-deduplication.
/// </summary>
/// <remarks>
///  Per-second emit iff <c>Q.sfm[i] &gt; SfmThreshold AND T.sfm[i] &gt; SfmThreshold</c> (presence, pointwise) and the
///  windowed mean of <c>|Q.sfm - T.sfm|</c> over ±<see cref="AgreementWindowRadius"/> seconds is &lt; SimilarityTolerance
///  (agreement). The defaults (SfmThreshold=0.40, AgreementWindowRadius=2) admit moderate-entropy filler while the
///  windowed agreement rejects coincidental single-second matches from different content. Emissions are truncated in
///  Phase 2 at <see cref="MaxQueryRelativeBridge"/> × queryLength (default 0.70).
/// </remarks>
public sealed class BroadbandNoiseBridgingStrategy : ISfmMatchStrategy
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="BroadbandNoiseBridgingStrategy"/> class.
    /// </summary>
    /// <param name="sfmThreshold">
    ///  Minimum SFM for a second to be considered "broadband" on either side. Default 0.40 — paired with the windowed
    ///  agreement this admits moderate-entropy filler that a higher pointwise bar would miss. Valid range [0.20, 0.90].
    /// </param>
    /// <param name="similarityTolerance">
    ///  Maximum allowed mean |ΔSFM| between query and track across the agreement window. Default 0.15. Valid range [0.05, 0.30].
    /// </param>
    /// <param name="maxQueryRelativeBridge">
    ///  Maximum fraction of the query that may be filled with synthetic seconds. Default 0.70. Valid range [0.0, 1.0].
    ///  Raise it to bridge a noise-heavy clip whose real anchors carry less than 30% of the timeline.
    /// </param>
    /// <param name="agreementWindowRadius">
    ///  Radius (in seconds) over which the |ΔSFM| agreement is averaged. Default 2 — a windowed mean smooths genuine
    ///  per-second SFM jitter (so the lower <paramref name="sfmThreshold"/> still bridges real filler) and averages out
    ///  coincidental single-second agreements from different content (a splice). 0 = pointwise. Valid range [0, 30].
    /// </param>
    public BroadbandNoiseBridgingStrategy(double sfmThreshold = 0.40, double similarityTolerance = 0.15, double maxQueryRelativeBridge = 0.70, int agreementWindowRadius = 2)
    {
        if (sfmThreshold is < 0.20 or > 0.90)
        {
            throw new ArgumentOutOfRangeException(nameof(sfmThreshold), sfmThreshold, "Must be in [0.20, 0.90].");
        }

        if (similarityTolerance is < 0.05 or > 0.30)
        {
            throw new ArgumentOutOfRangeException(nameof(similarityTolerance), similarityTolerance, "Must be in [0.05, 0.30].");
        }

        if (maxQueryRelativeBridge is < 0.0 or > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxQueryRelativeBridge), maxQueryRelativeBridge, "Must be in [0.0, 1.0].");
        }

        if (agreementWindowRadius is < 0 or > 30)
        {
            throw new ArgumentOutOfRangeException(nameof(agreementWindowRadius), agreementWindowRadius, "Must be in [0, 30].");
        }

        SfmThreshold = sfmThreshold;
        SimilarityTolerance = similarityTolerance;
        MaxQueryRelativeBridge = maxQueryRelativeBridge;
        AgreementWindowRadius = agreementWindowRadius;
    }

    /// <summary>
    ///  Gets the singleton instance with calibrated defaults (SfmThreshold=0.40, SimilarityTolerance=0.15, AgreementWindowRadius=2).
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
    public double MaxQueryRelativeBridge { get; }

    /// <inheritdoc />
    public double MaxAbsoluteBridgeSeconds => double.PositiveInfinity;

    /// <summary>
    ///  Gets the radius (seconds) over which the |ΔSFM| agreement is averaged. 0 = pointwise.
    /// </summary>
    public int AgreementWindowRadius { get; }

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
            IsBroadbandMatch,
            MatchedWithType.BroadbandNoise);
    }

    // presence is pointwise (both sides flat at this second); agreement is a windowed mean of |ΔSFM| — a window > 0
    // smooths genuine per-second SFM noise (so a lower floor still bridges real filler) and averages out coincidental
    // single-second agreements between different content (so a splice is refused). radius 0 reduces to the pointwise check.
    private bool IsBroadbandMatch(SpectralProfile queryProfile, SpectralProfile trackProfile, int queryIndex, int trackIndex)
    {
        var query = queryProfile.PerSecond;
        var track = trackProfile.PerSecond;
        if (query[queryIndex].Sfm <= SfmThreshold || track[trackIndex].Sfm <= SfmThreshold)
        {
            return false;
        }

        double diff = 0;
        int n = 0;
        for (int d = -AgreementWindowRadius; d <= AgreementWindowRadius; d++)
        {
            int qj = queryIndex + d;
            int tj = trackIndex + d;
            if (qj < 0 || qj >= query.Count || tj < 0 || tj >= track.Count)
            {
                continue;
            }

            diff += Math.Abs(query[qj].Sfm - track[tj].Sfm);
            n++;
        }

        return n > 0 && (diff / n) < SimilarityTolerance;
    }
}