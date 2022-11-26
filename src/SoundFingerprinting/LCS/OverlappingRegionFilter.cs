namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class OverlappingRegionFilter
    {
        /// <summary>
        ///  Filters coverages that are contained within longer coverages.
        /// </summary>
        /// <param name="sequences">List of coverages to check.</param>
        /// <returns>Same or smaller list of unique longest coverages.</returns>
        public static IEnumerable<Coverage> FilterContainedCoverages(IEnumerable<Coverage> sequences)
        {
            var coverages = sequences
                .OrderByDescending(_ => _.TrackDiscreteCoverageLength)
                .ThenByDescending(_ => _.QueryDiscreteCoverageLength)
                .ToList();

            bool[] within = new bool[coverages.Count];
            for (int i = 0; i < coverages.Count - 1; ++i)
            {
                if (within[i])
                {
                    // if current sequence was detected as within a different sequence, we don't need to check sibling sequences,
                    // as they will get caught by parent coverage that is larger as measured by track/query coverage.
                    continue;
                }
                
                for (int j = i + 1; j < coverages.Count; ++j)
                {
                    if (coverages[i].Contains(coverages[j], delta: coverages[i].FingerprintLength))
                    {
                        within[j] = true;
                    }
                }
            }

            return coverages.Where((_, index) => !within[index]).ToList();
        }
    }
}
