namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.SFM;
    using static SoundFingerprinting.LCS.QueryPathReconstructionStrategyType;

    /// <summary>
    ///   Enumerable matched with extensions.
    /// </summary>
    public static class MatchedWithExtensions
    {
        private const double PermittedGapZero = 1e-5;

        private static readonly IQueryPathReconstructionStrategy QueryPathReconstructionStrategy = new QueryPathReconstructionStrategy();

        /// <summary>
        ///  Get coverages from provided best path described by matched with list.
        /// </summary>
        /// <param name="matchedEntries">Best path as described by matched with list.</param>
        /// <param name="queryPathReconstructionStrategyType">Query path reconstruction strategy type.</param>
        /// <param name="queryLength">Query length.</param>
        /// <param name="trackLength">Track length.</param>
        /// <param name="fingerprintLength">Fingerprint length.</param>
        /// <param name="permittedGap">Permitted gap.</param>
        /// <returns>List of calculated coverages.</returns>
        /// <exception cref="NotSupportedException">Provided <paramref name="queryPathReconstructionStrategyType"/> is not supported.</exception>
        public static IEnumerable<Coverage> GetCoverages(
            this IEnumerable<MatchedWith> matchedEntries,
            QueryPathReconstructionStrategyType queryPathReconstructionStrategyType,
            double queryLength,
            double trackLength,
            double fingerprintLength,
            double permittedGap)
        {
            return matchedEntries.GetCoverages(
                queryPathReconstructionStrategyType,
                queryLength,
                trackLength,
                fingerprintLength,
                permittedGap,
                NoBridgingStrategy.Default,
                queryProfile: null,
                trackProfile: null);
        }

        /// <summary>
        ///  Get coverages with SFM-based path bridging.
        /// </summary>
        /// <remarks>
        ///  When <paramref name="sfmMatchStrategy"/> is <see cref="NoBridgingStrategy"/> or either profile is <c>null</c>,
        ///  or <paramref name="queryPathReconstructionStrategyType"/> is <see cref="Legacy"/>, behaviour is identical
        ///  to the no-bridging overload. Otherwise: Phase 1 emits candidates, Phase 2 derives sequence numbers via
        ///  bracket interpolation and applies the strategy's MaxQueryRelativeBridge cap, then synthetics are concatenated with
        ///  real matches before LIS path reconstruction. Each resulting <see cref="Coverage.BridgedSeconds"/> reports
        ///  how many synthetics survived into the final best path.
        /// </remarks>
        /// <param name="matchedEntries">Real (hash-derived) matches.</param>
        /// <param name="queryPathReconstructionStrategyType">Path reconstruction strategy type.</param>
        /// <param name="queryLength">Query length.</param>
        /// <param name="trackLength">Track length.</param>
        /// <param name="fingerprintLength">Fingerprint length.</param>
        /// <param name="permittedGap">Permitted gap.</param>
        /// <param name="sfmMatchStrategy">SFM match strategy. <see cref="NoBridgingStrategy"/> disables bridging.</param>
        /// <param name="queryProfile">Query spectral profile (decoded). <c>null</c> disables bridging.</param>
        /// <param name="trackProfile">Track spectral profile (decoded). <c>null</c> disables bridging.</param>
        /// <returns>List of calculated coverages.</returns>
        public static IEnumerable<Coverage> GetCoverages(
            this IEnumerable<MatchedWith> matchedEntries,
            QueryPathReconstructionStrategyType queryPathReconstructionStrategyType,
            double queryLength,
            double trackLength,
            double fingerprintLength,
            double permittedGap,
            ISfmMatchStrategy sfmMatchStrategy,
            SpectralProfile? queryProfile,
            SpectralProfile? trackProfile)
        {
            var realMatches = matchedEntries as IList<MatchedWith> ?? matchedEntries.ToList();
            bool bridgingActive = queryPathReconstructionStrategyType != Legacy
                                  && sfmMatchStrategy is not NoBridgingStrategy
                                  && queryProfile != null
                                  && trackProfile != null
                                  && realMatches.Count > 0;

            HashSet<MatchedWith>? synthetics = null;
            IEnumerable<MatchedWith> augmented = realMatches;
            if (bridgingActive)
            {
                // sort once here so both Phase 1 (bracket lookup in SyntheticCandidateUtils) and Phase 2 (skip-if-real + seq derivation) walk a sorted list
                var sortedReals = realMatches.OrderBy(r => r.QueryMatchAt).ToList();
                var context = new SyntheticMatchContext(
                    sortedReals,
                    queryProfile!,
                    trackProfile!,
                    queryLength,
                    trackLength,
                    fingerprintLength);
                var emitted = EmitSynthetics(sfmMatchStrategy, context, sortedReals);
                if (emitted.Count > 0)
                {
                    synthetics = new HashSet<MatchedWith>(emitted);
                    augmented = realMatches.Concat(emitted);
                }
            }

            var reconstructedPaths = queryPathReconstructionStrategyType switch
            {
                Legacy => new LegacyQueryPathReconstructionStrategy(fingerprintLength).GetBestPaths(augmented, Math.Min(queryLength, trackLength)),
                MultipleBestPaths => QueryPathReconstructionStrategy.GetBestPaths(augmented, permittedGap),
                _ => throw new NotSupportedException($"Provided path reconstruction strategy is not valid {queryPathReconstructionStrategyType}")
            };

            var coverages = reconstructedPaths
                .Select(sequence =>
                {
                    var path = sequence as IList<MatchedWith> ?? sequence.ToList();
                    int bridged = synthetics == null ? 0 : path.Count(m => synthetics.Contains(m));
                    return new Coverage(path, queryLength, trackLength, fingerprintLength, permittedGap, bridged);
                })
                .ToList();

            return queryPathReconstructionStrategyType switch
            {
                MultipleBestPaths => OverlappingRegionFilter.FilterContainedCoverages(coverages),
                _ => coverages
            };
        }

        /// <summary>
        ///  Find query gaps.
        /// </summary>
        /// <param name="entries">Bets path as described by list of matched with entries.</param>
        /// <param name="queryLength">Query length.</param>
        /// <param name="permittedGap">Permitted gap.</param>
        /// <param name="fingerprintLength">Fingerprint length.</param>
        /// <returns>List of gaps (if any).</returns>
        public static IEnumerable<Gap> FindQueryGaps(this IEnumerable<MatchedWith> entries, double queryLength, double permittedGap, double fingerprintLength)
        {
            // ordering is redundant for QueryPathReconstructionStrategyType.SingleBestPath and QueryPathReconstructionStrategyType.MultipleBestPaths
            var ordered = entries
                .OrderBy(entry => entry.QueryMatchAt)
                .Select(m => Tuple.Create(m.QuerySequenceNumber, m.QueryMatchAt))
                .ToList();

            return FindGaps(ordered, queryLength, permittedGap, fingerprintLength);
        }

        /// <summary>
        ///  Find track gaps.
        /// </summary>
        /// <param name="entries">Bets path as described by list of matched with entries.</param>
        /// <param name="trackLength">Track length</param>
        /// <param name="permittedGap">Permitted gap.</param>
        /// <param name="fingerprintLength">Fingerprint length.</param>
        /// <returns>List of gaps (if any).</returns>
        public static IEnumerable<Gap> FindTrackGaps(this IEnumerable<MatchedWith> entries, double trackLength, double permittedGap, double fingerprintLength)
        {
            // ordering is redundant for QueryPathReconstructionStrategyType.SingleBestPath and QueryPathReconstructionStrategyType.MultipleBestPaths
            var ordered = entries.OrderBy(m => m.TrackMatchAt)
                .Select(m => Tuple.Create(m.TrackSequenceNumber, m.TrackMatchAt))
                .ToList();

            return FindGaps(ordered, trackLength, permittedGap, fingerprintLength);
        }

        /// <summary>
        ///  Splits best path by maximum gap.
        /// </summary>
        /// <param name="matches">Matches to split.</param>
        /// <param name="maxGap">Maximum gap to consider.</param>
        /// <returns>List of split best paths.</returns>
        public static IEnumerable<IEnumerable<MatchedWith>> SplitBestPathByMaxGap(this IEnumerable<MatchedWith> matches, double maxGap)
        {
            var sequence = matches as MatchedWith[] ?? matches.ToArray();
            if (!sequence.Any())
            {
                return Enumerable.Empty<IEnumerable<MatchedWith>>();
            }

            int start = 0;
            var list = new List<IEnumerable<MatchedWith>>();
            for (int index = 1; index < sequence.Length; ++index)
            {
                if (Math.Abs(sequence[index].QueryMatchAt - sequence[index - 1].QueryMatchAt) > maxGap || Math.Abs(sequence[index].TrackMatchAt - sequence[index - 1].TrackMatchAt) > maxGap)
                {
                    list.Add(sequence.Skip(start).Take(index - start));
                    start = index;
                }
            }

            var last = sequence.Skip(start).ToList();
            if (last.Any())
            {
                list.Add(last);
            }

            return list;
        }

        // Phase 2: skip-if-real → bracket interpolation → per-strategy MaxQueryRelativeBridge cap → MatchedWith construction
        private static List<MatchedWith> EmitSynthetics(ISfmMatchStrategy strategy, SyntheticMatchContext context, List<MatchedWith> reals)
        {
            // the forward-cursor bracket lookup below requires candidates ascending by QueryMatchAt (ISfmMatchStrategy postcondition).
            // EmitPerSecondCandidates and CompositeBridgingStrategy already honour it; sort defensively
            // so a future strategy that violates the contract degrades to wrong tuning, never corrupt bracketing.
            var candidates = strategy.GenerateCandidates(context).OrderBy(c => c.QueryMatchAt).ToList();
            if (candidates.Count == 0)
            {
                return new List<MatchedWith>();
            }

            // per-strategy false-positive backstop: cap synthetics at min(absolute ceiling, relative fraction of query)
            int universalCap = (int)Math.Floor(Math.Min(strategy.MaxAbsoluteBridgeSeconds, strategy.MaxQueryRelativeBridge * context.QueryLength));

            // local slope at each endpoint for one-sided (head/tail) bridges. Returns 0 if the slope can't be derived;
            // TryDeriveSeq treats slot period <= 0 as "refuse to bridge" rather than falling back to a magic constant.
            double headSlotPeriod = LocalSlotPeriod(reals, 0, 1);
            double tailSlotPeriod = LocalSlotPeriod(reals, reals.Count - 2, reals.Count - 1);

            var emitted = new List<MatchedWith>();
            int bridged = 0;
            int realIndex = 0;
            var perGapSyntheticCount = new Dictionary<(int LeftSeq, int RightSeq), (int UsedQ, int UsedT)>();
            foreach (var c in candidates)
            {
                if (bridged >= universalCap)
                {
                    break;
                }

                // bracket lookup via monotonic forward-sweep cursor (O(N+M) total across all candidates)
                while (realIndex < reals.Count && reals[realIndex].QueryMatchAt <= c.QueryMatchAt)
                {
                    realIndex++;
                }

                MatchedWith? leftReal = realIndex > 0 ? reals[realIndex - 1] : null;
                MatchedWith? rightReal = realIndex < reals.Count ? reals[realIndex] : null;

                // skip-if-real: leftReal (<= c.QueryMatchAt) lands on the second iff its QueryMatchAt == c.QueryMatchAt;
                // rightReal (> c.QueryMatchAt) lands on the second iff its QueryMatchAt is in (c.QueryMatchAt, c.QueryMatchAt + 1)
                bool coveredByLeft = leftReal != null && leftReal.QueryMatchAt >= c.QueryMatchAt;
                bool coveredByRight = rightReal != null && rightReal.QueryMatchAt < c.QueryMatchAt + 1;
                if (coveredByLeft || coveredByRight)
                {
                    continue;
                }

                if (!TryDeriveSeq(c, leftReal, rightReal, headSlotPeriod, tailSlotPeriod, perGapSyntheticCount, out uint qSynth, out uint tSynth))
                {
                    continue;
                }

                emitted.Add(new MatchedWith(qSynth, (float)c.QueryMatchAt, tSynth, (float)c.TrackMatchAt, score: 1));
                bridged++;
            }

            return emitted;
        }

        // local seconds-per-sub-fingerprint between two adjacent reals, derived from the query axis. Returns 0 if the
        // span doesn't admit a slope (single-real input, duplicate seq numbers, or out-of-range indices) — callers
        // treat 0 as "can't bridge here" instead of falling back to a magic constant.
        private static double LocalSlotPeriod(List<MatchedWith> reals, int leftIdx, int rightIdx)
        {
            if (leftIdx < 0 || rightIdx >= reals.Count || leftIdx >= rightIdx)
            {
                // single-real fallback: derive from origin only if both seq and time are strictly positive
                if (reals.Count == 1 && reals[0].QuerySequenceNumber > 0 && reals[0].QueryMatchAt > 0)
                {
                    return reals[0].QueryMatchAt / reals[0].QuerySequenceNumber;
                }

                return 0;
            }

            long qSeqSpan = (long)reals[rightIdx].QuerySequenceNumber - reals[leftIdx].QuerySequenceNumber;
            double qTimeSpan = reals[rightIdx].QueryMatchAt - reals[leftIdx].QueryMatchAt;
            if (qSeqSpan <= 0 || qTimeSpan <= 0)
            {
                return 0;
            }

            return qTimeSpan / qSeqSpan;
        }

        private static bool TryDeriveSeq(
            SyntheticCandidate candidate,
            MatchedWith? leftReal,
            MatchedWith? rightReal,
            double headSlotPeriod,
            double tailSlotPeriod,
            Dictionary<(int LeftSeq, int RightSeq), (int UsedQ, int UsedT)> perGapSyntheticCount,
            out uint qSynth,
            out uint tSynth)
        {
            qSynth = 0;
            tSynth = 0;
            double trackTime = candidate.TrackMatchAt;
            if (leftReal != null && rightReal != null)
            {
                int qSpan = (int)rightReal.QuerySequenceNumber - (int)leftReal.QuerySequenceNumber;
                int tSpan = (int)rightReal.TrackSequenceNumber - (int)leftReal.TrackSequenceNumber;

                // qSpan/tSpan <= 0 catches anti-correlated brackets (right's seq lands at or before left's) — e.g.,
                // duplicate hash hits on the same sub-fingerprint, or reals from a manually-constructed test set.
                // It does NOT catch the multi-cluster case where the bracketing reals come from different alignments
                // (both clusters increase monotonically) — those phantom-diagonal synthetics are bounded by the
                // strategy's MaxQueryRelativeBridge cap and discarded by LIS post-hoc.
                if (qSpan <= 0 || tSpan <= 0)
                {
                    return false;
                }

                double qBracketSpan = rightReal.QueryMatchAt - leftReal.QueryMatchAt;
                double tBracketSpan = rightReal.TrackMatchAt - leftReal.TrackMatchAt;
                if (qBracketSpan <= 0 || tBracketSpan <= 0)
                {
                    return false;
                }

                double qFraction = (candidate.QueryMatchAt - leftReal.QueryMatchAt) / qBracketSpan;
                double tFraction = (trackTime - leftReal.TrackMatchAt) / tBracketSpan;
                int qOffset = (int)Math.Round(qFraction * qSpan);
                int tOffset = (int)Math.Round(tFraction * tSpan);
                var gapKey = ((int)leftReal.QuerySequenceNumber, (int)rightReal.QuerySequenceNumber);
                var used = perGapSyntheticCount.TryGetValue(gapKey, out var u) ? u : (UsedQ: 0, UsedT: 0);

                // both axes must strictly increase over previously-emitted synthetics in this gap
                if (qOffset <= used.UsedQ)
                {
                    qOffset = used.UsedQ + 1;
                }

                if (tOffset <= used.UsedT)
                {
                    tOffset = used.UsedT + 1;
                }

                if (qOffset >= qSpan || tOffset >= tSpan)
                {
                    return false;
                }

                qSynth = (uint)((int)leftReal.QuerySequenceNumber + qOffset);
                tSynth = (uint)((int)leftReal.TrackSequenceNumber + tOffset);
                perGapSyntheticCount[gapKey] = (qOffset, tOffset);
                return true;
            }

            if (leftReal == null && rightReal != null)
            {
                if (headSlotPeriod <= 0)
                {
                    return false;
                }

                int q = (int)Math.Round(candidate.QueryMatchAt / headSlotPeriod);
                int t = (int)Math.Round(trackTime / headSlotPeriod);
                if (q < 0 || t < 0 || q >= rightReal.QuerySequenceNumber || t >= rightReal.TrackSequenceNumber)
                {
                    return false;
                }

                qSynth = (uint)q;
                tSynth = (uint)t;
                return true;
            }

            if (leftReal != null && rightReal == null)
            {
                if (tailSlotPeriod <= 0)
                {
                    return false;
                }

                int qOffset = (int)Math.Round((candidate.QueryMatchAt - leftReal.QueryMatchAt) / tailSlotPeriod);
                int tOffset = (int)Math.Round((trackTime - leftReal.TrackMatchAt) / tailSlotPeriod);
                if (qOffset <= 0 || tOffset <= 0)
                {
                    return false;
                }

                qSynth = (uint)((int)leftReal.QuerySequenceNumber + qOffset);
                tSynth = (uint)((int)leftReal.TrackSequenceNumber + tOffset);
                return true;
            }

            // no anchors at all — caller should have refused entry, but guard anyway
            return false;
        }

        private static IEnumerable<Gap> FindGaps(IEnumerable<Tuple<uint, float>> ordered, double totalLength, double permittedGap, double fingerprintLength)
        {
            double sanitizedPermittedGap = permittedGap > 0 ? permittedGap : PermittedGapZero;
            var tuples = ordered.ToList();
            (_, float startsAt) = tuples.First();
            if (startsAt > sanitizedPermittedGap)
            {
                yield return new Gap(0, startsAt, true);
            }

            foreach (var gap in FindGaps(tuples, sanitizedPermittedGap, fingerprintLength))
            {
                yield return gap;
            }

            (_, float end) = tuples.Last();

            double endsAt = end + fingerprintLength;
            if (totalLength - endsAt > sanitizedPermittedGap)
            {
                yield return new Gap(endsAt, totalLength, true);
            }
        }

        private static IEnumerable<Gap> FindGaps(IEnumerable<Tuple<uint, float>> entries, double permittedGap, double fingerprintLength)
        {
            Tuple<uint, float>[] matches = entries.ToArray();
            for (int i = 1; i < matches.Length; ++i)
            {
                float startsAt = matches[i - 1].Item2;
                float endsAt = matches[i].Item2;
                float gap = (float)SubFingerprintsToSeconds.GapLengthToSeconds(endsAt, startsAt, fingerprintLength);
                bool sequenceNumberIncremented = matches[i].Item1 - matches[i - 1].Item1 > 1;
                float start = endsAt - gap;
                if (!(endsAt <= start) && gap > permittedGap && sequenceNumberIncremented)
                {
                    yield return new Gap(start, endsAt, false);
                }
            }
        }
    }
}
