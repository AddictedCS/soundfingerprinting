namespace SoundFingerprinting.SFM;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///  Composes multiple <see cref="ISfmMatchStrategy"/> instances by taking the union of their
///  per-second emissions. Candidates emitted at the same <c>(QueryMatchAt, TrackMatchAt)</c>
///  by more than one inner strategy are deduplicated.
/// </summary>
/// <remarks>
///  Safe to compose: <see cref="BroadbandNoiseBridgingStrategy"/> and <see cref="SilentRegionBridgingStrategy"/> —
///  both rely on the universal 70% cumulative cap in Phase 2 as their only backstop, and their per-second predicates
///  are nearly mutually exclusive in practice (high SFM rarely coincides with low power).
///  <para>
///  Unsafe to compose: <see cref="SimilarProfileBridgingStrategy"/>. Its load-bearing safety is its own
///  <c>min(10s, 0.30 × queryLength)</c> cap; composing under a union strategy would lift its emissions onto the
///  far more permissive universal 70% cap. The constructor rejects this combination.
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
    ///  <see cref="SimilarProfileBridgingStrategy"/>, <see cref="NoBridgingStrategy"/>,
    ///  or nested <see cref="CompositeBridgingStrategy"/> instances.</param>
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
                case SimilarProfileBridgingStrategy:
                    throw new ArgumentException(
                        "SimilarProfileBridgingStrategy cannot be composed — its per-strategy cumulative cap is load-bearing safety. Use it standalone.",
                        nameof(strategies));
                case NoBridgingStrategy:
                    throw new ArgumentException("NoBridgingStrategy is the empty bridge; composing with it is meaningless.", nameof(strategies));
                case CompositeBridgingStrategy:
                    throw new ArgumentException("Nesting CompositeBridgingStrategy is not allowed; flatten the inner strategies at construction.", nameof(strategies));
            }
        }

        inner = strategies.ToList();
    }

    /// <summary>
    ///  Gets the singleton composing <see cref="BroadbandNoiseBridgingStrategy.Default"/> and
    ///  <see cref="SilentRegionBridgingStrategy.Default"/> — covers both broadband regions
    ///  (waves, action SFX) and silent regions (fade-outs, dramatic pauses) in one pass.
    /// </summary>
    public static CompositeBridgingStrategy BroadbandOrSilent { get; } = new (BroadbandNoiseBridgingStrategy.Default, SilentRegionBridgingStrategy.Default);

    /// <summary>
    ///  Gets the inner strategies in iteration order.
    /// </summary>
    public IReadOnlyList<ISfmMatchStrategy> InnerStrategies => inner;

    /// <inheritdoc />
    public IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context)
    {
        var seen = new HashSet<(double Q, double T)>();
        foreach (var strategy in inner)
        {
            foreach (var candidate in strategy.GenerateCandidates(context))
            {
                if (seen.Add((candidate.QueryMatchAt, candidate.TrackMatchAt)))
                {
                    yield return candidate;
                }
            }
        }
    }
}
