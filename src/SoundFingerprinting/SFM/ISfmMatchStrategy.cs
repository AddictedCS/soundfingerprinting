namespace SoundFingerprinting.SFM;

using System.Collections.Generic;
using SoundFingerprinting.Query;

/// <summary>
///  Generates per-second synthetic match candidates in regions where the spectral character (SFM, power, or both)
///  of the query and track agree under the strategy's per-second check.
/// </summary>
/// <remarks>
///  Phase 1 of two-phase emission: the strategy only returns per-second (Q, T) time pairs that
///  pass its spectral check. Phase 2 — <see cref="MatchedWithExtensions"/> — derives sequence
///  numbers via bracket interpolation, applies skip-if-real, enforces the strategy's
///  <see cref="MaxQueryRelativeBridge"/> cap, and constructs the final <see cref="MatchedWith"/> records.
///  Strategies never receive <c>null</c> profiles — the caller short-circuits to no-bridging when either profile is missing.
/// </remarks>
public interface ISfmMatchStrategy
{
    /// <summary>
    ///  Gets the maximum fraction of the query that this strategy may fill with synthetic (bridged) seconds.
    ///  Phase 2 (<see cref="MatchedWithExtensions"/>) truncates emitted synthetics at
    ///  <c>floor(min(MaxAbsoluteBridgeSeconds, MaxQueryRelativeBridge × queryLength))</c> — the false-positive backstop
    ///  that keeps a match from being dominated by interpolated content rather than real hash hits. Default 0.70 for the
    ///  noise/silence strategies; a caller that genuinely needs to bridge a noise-heavy clip past that supplies a higher
    ///  value via the strategy constructor. <see cref="NoBridgingStrategy"/> reports 0.
    /// </summary>
    double MaxQueryRelativeBridge { get; }

    /// <summary>
    ///  Gets the absolute ceiling (in seconds) on synthetic content, combined with <see cref="MaxQueryRelativeBridge"/>
    ///  as <c>min(MaxAbsoluteBridgeSeconds, MaxQueryRelativeBridge × queryLength)</c> in Phase 2. Strategies with no
    ///  absolute bound report <see cref="double.PositiveInfinity"/>; <see cref="SimilarProfileBridgingStrategy"/> reports
    ///  its calibrated absolute cap — the term that keeps a weakly-anchored bridge from crossing the merge threshold.
    /// </summary>
    double MaxAbsoluteBridgeSeconds { get; }

    /// <summary>
    ///  Generate per-second bridge candidates.
    /// </summary>
    /// <param name="context">Per-call context with real matches, both profiles, and lengths.</param>
    /// <returns>
    ///  Candidates that pass the strategy's spectral check, optionally truncated by the strategy's own cumulative cap.
    ///  <para>
    ///  Postcondition: the returned candidates MUST be sorted ascending by <see cref="SyntheticCandidate.QueryMatchAt"/>.
    ///  Phase 2 (<see cref="MatchedWithExtensions"/>) walks them with a monotonic forward cursor for bracket lookup and
    ///  relies on this ordering. <see cref="SyntheticCandidateUtils.EmitPerSecondCandidates"/> already emits in this order;
    ///  composite implementations must re-sort their merged union.
    ///  </para>
    /// </returns>
    IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context);
}