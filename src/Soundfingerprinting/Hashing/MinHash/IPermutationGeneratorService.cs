namespace Soundfingerprinting.Hashing.MinHash
{
    using System.Collections.Generic;

    public interface IPermutationGeneratorService
    {
        /// <summary>
        ///   Get unique aggresive permutations. 
        ///   E.g. Consider a set of [0, .., 8191] elements that can defined within any of the permutations that contain 255 elements.
        ///   As a result we can make the 8192/255 unique permutations that will not contain duplicated indexes.
        /// </summary>
        /// <param name = "tables">L tables [E.g. 20]</param>
        /// <param name = "keysPerTable">K keys per table [E.g. 5]</param>
        /// <param name = "startIndex">Start index in the permutation set</param>
        /// <param name = "endIndex">End index in the permutation set</param>
        /// <returns>Dictionary with index of permutation and the list itself</returns>
        Dictionary<int, int[]> GenerateRandomPermutationsUsingUniqueIndexes(int tables, int keysPerTable, int startIndex, int endIndex);

        /// <summary>
        /// Generate permutations using Minimal Mutual Information Approach
        /// </summary>
        /// <param name="tables">
        /// L tables [E.g. 20]
        /// </param>
        /// <param name="keysPerTable">
        /// K keys per table [E.g. 5]
        /// </param>
        /// <param name="startIndex">
        /// Start index in the permutation set
        /// </param>
        /// <param name="endIndex">
        /// End index in the permutation set
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <returns>
        /// Dictionary with index of permutation and the list itself
        /// </returns>
        Dictionary<int, int[]> GeneratePermutationsUsingMinMutualInformation(int tables, int keysPerTable, int startIndex, int endIndex, IMinMutualSelector selector);

        /// <summary>
        ///   Get unique aggresive permutations. 
        ///   E.g. Consider a set of [0, .., 8191] elements that can defined within any of the permutations that contain 255 elements.
        ///   As a result we can make the 8192/255 unique permutations that will not contain duplicated indexes.
        /// </summary>
        /// <param name = "totalCount">Total count of the permutation pool, that is needed to be generated [E.g. 100]</param>
        /// <param name = "elementsPerPermutation">Number of elements per permutation [E.g. 255]</param>
        /// <param name = "startIndex">Start index of the permutation [E.g. 0]</param>
        /// <param name = "endIndex">End index of the permutation [E.g. 8192]</param>
        /// <returns>Permutation pool with most unique indexes within permutation set</returns>
        Dictionary<int, int[]> GetAgressiveUniqueRandomPermutations(int totalCount, int elementsPerPermutation, int startIndex, int endIndex);

        /// <summary>
        ///   Get random permutations
        /// </summary>
        /// <param name = "totalCount">Number of permutations that are needed for the generation</param>
        /// <param name = "elementsPerPermutation">Elements per permutation</param>
        /// <param name = "startIndex">Start index</param>
        /// <param name = "endIndex">End index</param>
        /// <returns>Permutation pool</returns>
        Dictionary<int, int[]> GetRandomPermutations(int totalCount, int elementsPerPermutation, int startIndex, int endIndex);

        /// <summary>
        /// Random shuffle of the elements in place
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <typeparam name="T">
        /// T type of the elements in the enumeration
        /// </typeparam>
        void RandomShuffleInPlace<T>(T[] array);
    }
}