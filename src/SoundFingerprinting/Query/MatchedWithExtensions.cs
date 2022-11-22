namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///   Enumerable matched with extensions.
    /// </summary>
    public static class MatchedWithExtensions
    {
        private const double PermittedGapZero = 1e-5;
        
        private static readonly IQueryPathReconstructionStrategy Single = new SingleQueryPathReconstructionStrategy();
        private static readonly IQueryPathReconstructionStrategy Multiple = new MultipleQueryPathReconstructionStrategy();
        
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
                QueryPathReconstructionStrategyType.Legacy => new LegacyQueryPathReconstructionStrategy(fingerprintLength).GetBestPaths(matchedEntries, Math.Min(queryLength, trackLength)),
                QueryPathReconstructionStrategyType.SingleBestPath => Single.GetBestPaths(matchedEntries, -1),
                QueryPathReconstructionStrategyType.MultipleBestPaths => Multiple.GetBestPaths(matchedEntries, permittedGap),
                _ => throw new NotSupportedException($"Provided path reconstruction strategy is not valid {queryPathReconstructionStrategyType}")
            };
            
            var coverages = reconstructedPaths.Select(sequence => new Coverage(sequence, queryLength, trackLength, fingerprintLength, permittedGap));
            if (queryPathReconstructionStrategyType == QueryPathReconstructionStrategyType.MultipleBestPaths)
            {
                return OverlappingRegionFilter.FilterContainedCoverages(coverages); 
            }

            return coverages.ToList();
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
