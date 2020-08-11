namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    public static class OverlappingRegionFilter
    {
        public static IEnumerable<Coverage> FilterContainedCoverages(IEnumerable<Coverage> sequences)
        {
            var coverages = sequences
                .OrderByDescending(_ => _.CoverageWithPermittedGapsLength)
                .ThenByDescending(_ => _.QueryCoverageWithPermittedGapsLength)
                .ToList();
            
            bool[] within = new bool[coverages.Count];
            for (int i = 0; i < coverages.Count - 1; ++i)
            {
                for (int j = i + 1; j < coverages.Count; ++j)
                {
                    if (coverages[i].Contains(coverages[j]))
                    {
                        within[j] = true;
                    }
                }
            }

            return coverages.Where((_, index) => !within[index]).ToList();
        }
        
        public static IEnumerable<Matches> MergeOverlappingSequences(List<Matches> sequences, double permittedGap)
        {
            for (int current = 0; current < sequences.Count; ++current)
            {
                for (int next = current + 1; next < sequences.Count; ++next)
                {
                    if (sequences[current].TryCollapseWith(sequences[next], permittedGap, out var c))
                    {
                        sequences.RemoveAt(next);
                        sequences.RemoveAt(current);
                        sequences.Add(c);
                        current = -1;
                        break;
                    }
                }
            }

            return FilterOverlappingMatches(sequences.OrderByDescending(sequence => sequence.EntriesCount));
        }

        private static IEnumerable<Matches> FilterOverlappingMatches(IEnumerable<Matches> sequences)
        {
            return sequences.Aggregate(new List<Matches>(), (results, matches) =>
            {
                foreach (var result in results)
                {
                    if (result.Contains(matches))
                    {
                        return results;
                    }
                }

                results.Add(matches);
                return results;
            })
            .OrderByDescending(sequence => sequence.EntriesCount);
        }
    }
}
