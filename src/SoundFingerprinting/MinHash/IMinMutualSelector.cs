namespace SoundFingerprinting.MinHash
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

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
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        Dictionary<int, List<int[]>> GetPermutations(Dictionary<int, int[]> randomPermutationPool, int lHashTable, int bKeysPerTable);
    }
}