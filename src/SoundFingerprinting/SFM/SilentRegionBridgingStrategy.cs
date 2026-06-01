namespace SoundFingerprinting.SFM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///  Bridges per-second regions where both query and track are silent (fade-outs, dramatic pauses,
    ///  atmospheric silence). Uses the power channel of <see cref="SoundFingerprinting.Audio.SpectralProfile"/>.
    /// </summary>
    /// <remarks>
    ///  Per-second emit iff <c>Q.power[i] &lt; PowerThreshold AND T.power[i] &lt; PowerThreshold AND |Q.power - T.power| &lt; PowerSimilarity</c>.
    ///  Power is bucket fraction relative to the per-audio maximum.
    /// </remarks>
    public sealed class SilentRegionBridgingStrategy : ISfmMatchStrategy
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="SilentRegionBridgingStrategy"/> class.
        /// </summary>
        /// <param name="powerThreshold">
        ///  Maximum power for a second to qualify as silent. Default 0.05. Valid range [0.01, 0.15].
        /// </param>
        /// <param name="powerSimilarity">
        ///  Maximum allowed |Δpower| between query and track at the same second. Default 0.05. Valid range [0.02, 0.15].
        /// </param>
        /// <param name="maxQueryRelativeBridge">
        ///  Maximum fraction of the query that may be filled with synthetic seconds. Default 0.70. Valid range [0.0, 1.0].
        /// </param>
        public SilentRegionBridgingStrategy(double powerThreshold = 0.05, double powerSimilarity = 0.05, double maxQueryRelativeBridge = 0.70)
        {
            if (powerThreshold < 0.01 || powerThreshold > 0.15)
            {
                throw new ArgumentOutOfRangeException(nameof(powerThreshold), powerThreshold, "Must be in [0.01, 0.15].");
            }

            if (powerSimilarity < 0.02 || powerSimilarity > 0.15)
            {
                throw new ArgumentOutOfRangeException(nameof(powerSimilarity), powerSimilarity, "Must be in [0.02, 0.15].");
            }

            if (maxQueryRelativeBridge < 0.0 || maxQueryRelativeBridge > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxQueryRelativeBridge), maxQueryRelativeBridge, "Must be in [0.0, 1.0].");
            }

            PowerThreshold = powerThreshold;
            PowerSimilarity = powerSimilarity;
            MaxQueryRelativeBridge = maxQueryRelativeBridge;
        }

        /// <summary>
        ///  Gets the singleton instance with calibrated defaults.
        /// </summary>
        public static SilentRegionBridgingStrategy Default { get; } = new ();

        /// <summary>
        ///  Gets the per-side silence power threshold.
        /// </summary>
        public double PowerThreshold { get; }

        /// <summary>
        ///  Gets the maximum allowed |Δpower| between query and track at the same second.
        /// </summary>
        public double PowerSimilarity { get; }

        /// <inheritdoc />
        public double MaxQueryRelativeBridge { get; }

        /// <inheritdoc />
        public double MaxAbsoluteBridgeSeconds => double.PositiveInfinity;

        /// <inheritdoc />
        public IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context)
        {
            if (context.RealMatches.Count == 0)
            {
                return Enumerable.Empty<SyntheticCandidate>();
            }

            return SyntheticCandidateUtils.EmitPerSecondCandidates(
                context.QueryProfile,
                context.TrackProfile,
                context.QueryLength,
                context.RealMatches,
                (query, track, queryIndex, trackIndex) =>
                {
                    // pointwise: silence is gated by the per-second "both quiet" presence test, not a windowed agreement
                    var qSecond = query.PerSecond[queryIndex];
                    var tSecond = track.PerSecond[trackIndex];
                    return qSecond.Power < PowerThreshold &&
                        tSecond.Power < PowerThreshold &&
                        Math.Abs(qSecond.Power - tSecond.Power) < PowerSimilarity;
                });
        }
    }
}
