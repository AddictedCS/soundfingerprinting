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
///  The merged union is capped at the strictest leg's budget — <see cref="MaxQueryRelativeBridge"/> and
///  <see cref="MaxAbsoluteBridgeSeconds"/> are both the minimum over the inner legs — so no leg's declared ceiling can
///  be exceeded by composition. <see cref="BroadbandNoiseBridgingStrategy"/> and <see cref="SilenceBridgingStrategy"/>
///  compose naturally (their per-second predicates are nearly mutually exclusive: high SFM rarely coincides with low power).
///  <para>
///  <see cref="SimilarProfileBridgingStrategy"/> is now composable: its calibrated absolute cap travels via
///  <see cref="MaxAbsoluteBridgeSeconds"/> and is preserved under the <c>min</c> combine, so a composite that includes it
///  inherits its tight <c>min(10s, 0.30 × queryLength)</c> bound rather than a more permissive one.
///  </para>
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
    ///  Derived as the minimum over the inner strategies. Because the Phase-2 cap bounds total synthetic seconds with
    ///  no per-gate attribution, the strictest leg's budget is the only value that keeps every leg's promise — no gate's
    ///  synthetics can exceed its own declared ceiling. To bridge a composite past a leg's cap, raise that leg.
    /// </remarks>
    public double MaxQueryRelativeBridge => inner.Min(s => s.MaxQueryRelativeBridge);

    /// <inheritdoc />
    /// <remarks>Minimum over the inner strategies — same strictest-leg-governs rule as <see cref="MaxQueryRelativeBridge"/>.</remarks>
    public double MaxAbsoluteBridgeSeconds => inner.Min(s => s.MaxAbsoluteBridgeSeconds);

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
