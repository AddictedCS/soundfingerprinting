namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    public static class OverlappingRegionFilter
    {
        /// <summary>
        ///  Filters coverages that are contained within longer coverages.
        /// </summary>
        /// <param name="sequences">List of coverages to check.</param>
        /// <returns>Same or smaller list of unique longest coverages.</returns>
        public static IEnumerable<Coverage> FilterContainedCoverages(IEnumerable<Coverage> sequences)
        {
            var coverages = sequences
                .OrderByDescending(_ => _.TrackCoverageWithPermittedGapsLength)
                .ThenByDescending(_ => _.QueryCoverageWithPermittedGapsLength)
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
                    // q  ---xxx----xxx---- 10
                    // t  ---xxx----xxx----
                    // c1 0 1 2 3 4 5 6 7 8 9 10 
                    //    0 1 2 3 4 5 6 7 8 9 10
                    // c2 2 3 4   
                    //    6 7 8
                    // c3 6 7 8
                    //    2 3 4
                    
                    // cross match with a gap
                    // q  ---xxx----------- 10
                    // t     xxx      xxx
                    // c1    2 3 4
                    //       2 3 4
                    // c2    2 3 4
                    //       6 7 8
                    if (coverages[i].Contains(coverages[j]))
                    {
                        within[j] = true;
                    }
                }
            }

            return coverages.Where((_, index) => !within[index]).ToList();
        }
    }
}
