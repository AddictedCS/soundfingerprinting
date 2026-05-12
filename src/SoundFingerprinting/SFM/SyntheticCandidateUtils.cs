namespace SoundFingerprinting.SFM;

using System;
using System.Collections.Generic;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Query;

/// <summary>
///  Shared per-second emission for <see cref="ISfmMatchStrategy"/> implementations.
///  For each query-second the corresponding track-second is derived from the local bracket of real matches:
///  linear interpolation when bracketed on both sides, unit-rate extrapolation from the single nearest anchor
///  at the head or tail. No global δ_time — the local diagonal handles multi-cluster real-match sets correctly.
/// </summary>
internal static class SyntheticCandidateUtils
{
    /// <summary>
    ///  Emit one candidate per query-second that passes the strategy's spectral check.
    /// </summary>
    /// <param name="queryProfile">Decoded query profile.</param>
    /// <param name="trackProfile">Decoded track profile.</param>
    /// <param name="queryLength">Query length in seconds.</param>
    /// <param name="realMatches">
    ///  Real matches between query and track. Must be ordered ascending by <see cref="MatchedWith.QueryMatchAt"/>
    ///  and must be non-empty (callers short-circuit on empty).
    /// </param>
    /// <param name="check">Strategy-supplied spectral check.</param>
    /// <returns>Per-second synthetic candidates anchored to the local query/track diagonal.</returns>
    public static IEnumerable<SyntheticCandidate> EmitPerSecondCandidates(
        SpectralProfile queryProfile,
        SpectralProfile trackProfile,
        double queryLength,
        IReadOnlyList<MatchedWith> realMatches,
        Func<SpectralSecond, SpectralSecond, bool> check)
    {
        int querySeconds = Math.Min(queryProfile.PerSecond.Count, (int)Math.Floor(queryLength));
        int realIndex = 0;
        for (int qIndex = 0; qIndex < querySeconds; ++qIndex)
        {
            // monotonic forward cursor — sorted reals + sorted qIndex ⇒ O(N+M) total bracket walks
            while (realIndex < realMatches.Count && realMatches[realIndex].QueryMatchAt <= qIndex)
            {
                realIndex++;
            }

            MatchedWith? leftReal = realIndex > 0 ? realMatches[realIndex - 1] : null;
            MatchedWith? rightReal = realIndex < realMatches.Count ? realMatches[realIndex] : null;

            double trackSecond = InterpolateTrackSecond(qIndex, leftReal, rightReal);
            int tIndex = (int)Math.Floor(trackSecond);
            if (tIndex < 0 || tIndex >= trackProfile.PerSecond.Count)
            {
                continue;
            }

            if (check(queryProfile.PerSecond[qIndex], trackProfile.PerSecond[tIndex]))
            {
                yield return new SyntheticCandidate(qIndex, trackSecond);
            }
        }
    }

    private static double InterpolateTrackSecond(double qSecond, MatchedWith? leftReal, MatchedWith? rightReal)
    {
        if (leftReal != null && rightReal != null)
        {
            // middle bridge: linear interpolation on the local diagonal between the two bracket reals
            double qSpan = rightReal.QueryMatchAt - leftReal.QueryMatchAt;
            if (qSpan > 0)
            {
                double tSpan = rightReal.TrackMatchAt - leftReal.TrackMatchAt;
                return leftReal.TrackMatchAt + ((qSecond - leftReal.QueryMatchAt) * tSpan / qSpan);
            }

            // degenerate (duplicate QueryMatchAt) — fall through to left-only extrapolation
        }

        if (leftReal != null)
        {
            // tail bridge: extrapolate forward at unit rate from the nearest real
            return leftReal.TrackMatchAt + (qSecond - leftReal.QueryMatchAt);
        }

        if (rightReal != null)
        {
            // head bridge: extrapolate backward at unit rate from the nearest real
            return rightReal.TrackMatchAt - (rightReal.QueryMatchAt - qSecond);
        }

        // no anchors — strategies short-circuit on empty realMatches, so this is unreachable
        return qSecond;
    }
}
