﻿namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.LCS;
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
            var reconstructedPaths = queryPathReconstructionStrategyType switch
            {
                Legacy => new LegacyQueryPathReconstructionStrategy(fingerprintLength).GetBestPaths(matchedEntries, Math.Min(queryLength, trackLength), limit: -1),
                SingleBestPath => QueryPathReconstructionStrategy.GetBestPaths(matchedEntries, maxGap: Math.Min(queryLength, trackLength), limit: int.MaxValue),
                MultipleBestPaths => QueryPathReconstructionStrategy.GetBestPaths(matchedEntries, maxGap: permittedGap, limit: int.MaxValue),
                _ => throw new NotSupportedException($"Provided path reconstruction strategy is not valid {queryPathReconstructionStrategyType}")
            };
            
            var coverages = reconstructedPaths.Select(sequence => new Coverage(sequence, queryLength, trackLength, fingerprintLength, permittedGap)).ToList();
            return queryPathReconstructionStrategyType switch
            {
                SingleBestPath or MultipleBestPaths => OverlappingRegionFilter.FilterContainedCoverages(coverages),
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

            double endsAt =  end + fingerprintLength;
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
