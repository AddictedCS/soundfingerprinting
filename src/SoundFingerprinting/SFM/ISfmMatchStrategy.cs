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
///  numbers via bracket interpolation, applies skip-if-real, enforces the universal 70% sanity
///  bound, and constructs the final <see cref="MatchedWith"/> records. Strategies never receive
///  <c>null</c> profiles — the caller short-circuits to no-bridging when either profile is missing.
/// </remarks>
public interface ISfmMatchStrategy
{
    /// <summary>
    ///  Generate per-second bridge candidates.
    /// </summary>
    /// <param name="context">Per-call context with real matches, both profiles, and lengths.</param>
    /// <returns>
    ///  Candidates that pass the strategy's spectral check, optionally truncated by the strategy's own cumulative cap.
    /// </returns>
    IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context);
}