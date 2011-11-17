// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Collections.Generic;

namespace Soundfingerprinting.Hashing
{
    /// <summary>
    ///   Minimal mutual information selector
    /// </summary>
    public interface IMinMutualSelector
    {
        /// <summary>
        ///   Get permutations according to the selected algorithm
        /// </summary>
        /// <param name = "randomPermutationPool">Random permutation pool</param>
        /// <param name = "lHashTable">L hash tables</param>
        /// <param name = "bKeysPerTable">B keys per table</param>
        /// <returns>Permutation set</returns>
        Dictionary<int, List<int[]>> GetPermutations(Dictionary<int, int[]> randomPermutationPool, int lHashTable, int bKeysPerTable);
    }
}