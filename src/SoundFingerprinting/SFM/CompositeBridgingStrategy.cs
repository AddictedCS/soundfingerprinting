namespace SoundFingerprinting.SFM;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///  Composes multiple <see cref="ISfmMatchStrategy"/> instances by taking the union of their
///  per-second emissions. Candidates emitted at the same <c>(QueryMatchAt, TrackMatchAt)</c>
///  by more than one inner strategy are deduplicated, and the union is returned sorted ascending by
///  <c>QueryMatchAt</c>. The result is therefore independent of the order in which the inner strategies
///  are supplied — <c>Composite(a, b)</c> and <c>Composite(b, a)</c> produce identical candidates.
/// </summary>
/// <remarks>
///  The merged union inherits the most permissive leg's budget — <see cref="MaxQueryRelativeBridge"/> and
///  <see cref="MaxAbsoluteBridgeSeconds"/> are both the <c>max</c> over the inner legs — so composing strategies is
///  additive: adding a leg can only ever add bridging capacity, never starve a more permissive leg. (A <c>min</c> combine
///  would let a strict leg such as <see cref="SimilarProfileBridgingStrategy"/> clamp the broadband/silence legs down to
///  its own tight cap, dropping pairs that those legs would otherwise complete to a full match.) The per-strategy cap is a
///  loose compute/sanity backstop, not the correctness guard — false-positive safety lives downstream in the real-anchor
///  coverage filter (<c>ignoreBridgedCoverage</c>, realtime identification) and the dedup content gate, neither of which
///  is weakened by a more generous bridge budget here. <see cref="BroadbandNoiseBridgingStrategy"/> and
///  <see cref="SilenceBridgingStrategy"/> compose naturally (their per-second predicates are nearly mutually exclusive:
///  high SFM rarely coincides with low power).
///  <para>
///  Composing with <see cref="NoBridgingStrategy"/> is rejected (meaningless union with the empty bridge).
///  Nesting <see cref="CompositeBridgingStrategy"/> inside another <see cref="CompositeBridgingStrategy"/> is rejected
///  (flatten at construction instead).
///  </para>
/// </remarks>
public sealed class CompositeBridgingStrategy : ISfmMatchStrategy
{
    private readonly IReadOnlyList<ISfmMatchStrategy> inner;

    /// <summary>
    ///  Initializes a new instance of the <see cref="CompositeBridgingStrategy"/> class.
    /// </summary>
    /// <param name="strategies">Two or more inner strategies. Must not include
    ///  <see cref="NoBridgingStrategy"/> or nested <see cref="CompositeBridgingStrategy"/> instances.</param>
    /// <exception cref="ArgumentNullException"><paramref name="strategies"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Fewer than two strategies, or a disallowed strategy type was passed.</exception>
    public CompositeBridgingStrategy(params ISfmMatchStrategy[] strategies)
    {
        if (strategies == null)
        {
            throw new ArgumentNullException(nameof(strategies));
        }

        if (strategies.Length < 2)
        {
            throw new ArgumentException("CompositeBridgingStrategy requires at least two inner strategies.", nameof(strategies));
        }

        foreach (var strategy in strategies)
        {
            switch (strategy)
            {
                case null:
                    throw new ArgumentException("Inner strategies must not be null.", nameof(strategies));
                case NoBridgingStrategy:
                    throw new ArgumentException("NoBridgingStrategy is the empty bridge; composing with it is meaningless.", nameof(strategies));
                case CompositeBridgingStrategy:
                    throw new ArgumentException("Nesting CompositeBridgingStrategy is not allowed; flatten the inner strategies at construction.", nameof(strategies));
            }
        }

        inner = strategies.ToList();
    }

    /// <inheritdoc />
    /// <remarks>
    ///  Derived as the maximum over the inner strategies, so composition is additive — the most permissive leg governs and
    ///  adding a tighter leg never reduces the budget the other legs would have had on their own.
    /// </remarks>
    public double MaxQueryRelativeBridge => inner.Max(s => s.MaxQueryRelativeBridge);

    /// <inheritdoc />
    /// <remarks>Maximum over the inner strategies — same most-permissive-leg-governs rule as <see cref="MaxQueryRelativeBridge"/>.</remarks>
    public double MaxAbsoluteBridgeSeconds => inner.Max(s => s.MaxAbsoluteBridgeSeconds);

    /// <inheritdoc />
    public IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context)
    {
        // canonical union: dedup by (Q,T), then sort ascending by QueryMatchAt. Each inner strategy emits its
        // own per-second run already sorted, but concatenating two sorted runs is not globally sorted — so the
        // output must be re-sorted here. This guarantees the result is independent of inner-strategy order (the
        // "union" contract) and honours the ascending-QueryMatchAt postcondition that Phase 2's forward-cursor
        // bracket lookup relies on. Two strategies that emit the same query-second derive the same track-second
        // from the shared local diagonal, so dedup by (Q,T) leaves at most one candidate per query-second.
        var seen = new HashSet<(double Q, double T)>();
        var merged = new List<SyntheticCandidate>();
        foreach (var strategy in inner)
        {
            foreach (var candidate in strategy.GenerateCandidates(context))
            {
                if (seen.Add((candidate.QueryMatchAt, candidate.TrackMatchAt)))
                {
                    merged.Add(candidate);
                }
            }
        }

        merged.Sort((a, b) =>
        {
            int cmp = a.QueryMatchAt.CompareTo(b.QueryMatchAt);
            return cmp != 0 ? cmp : a.TrackMatchAt.CompareTo(b.TrackMatchAt);
        });
        return merged;
    }
}
