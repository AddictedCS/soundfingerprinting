namespace SoundFingerprinting.MinHash.Permutations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class PermutationGeneratorService : IPermutationGeneratorService
    {
        private const int PermutationPool = 150;
            
        private const int ElementsPerPermutation = 255;

        private static readonly Random Random = new Random(unchecked((int)(DateTime.Now.Ticks >> 4)));

        public delegate void CallbackPermGenerator(int counter);

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
        public Dictionary<int, int[]> GenerateRandomPermutationsUsingUniqueIndexes(int tables, int keysPerTable, int startIndex, int endIndex)
        {
            return GetAgressiveUniqueRandomPermutations(
                tables * keysPerTable, ElementsPerPermutation, startIndex, endIndex);
        }

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
        public Dictionary<int, int[]> GeneratePermutationsUsingMinMutualInformation(int tables, int keysPerTable, int startIndex, int endIndex, IMinMutualSelector selector)
        {
            Dictionary<int, int[]> randomperms = GetRandomPermutations(PermutationPool, ElementsPerPermutation, startIndex, endIndex);
            var groups = selector.GetPermutations(randomperms, tables, keysPerTable);
            Dictionary<int, int[]> result = new Dictionary<int, int[]>();
            /*Combine the L Groups between them, in order to get the final set of permutations*/
            int count = 0;
            foreach (KeyValuePair<int, List<int[]>> group in groups)
            {
                foreach (int[] perm in group.Value)
                {
                    result.Add(count++, perm);
                }
            }

            return result;
        }

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
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public Dictionary<int, int[]> GetAgressiveUniqueRandomPermutations(int totalCount, int elementsPerPermutation, int startIndex, int endIndex)
        {
            int seed = unchecked((int)(DateTime.Now.Ticks >> 4));
            Random random = new Random(seed);

            const int InitialRandomShuffling = 25;
            int[] possibleIndixes = new int[endIndex - startIndex]; /*Array of possible indixes*/
            for (int i = 0; i < endIndex - startIndex; i++)
            {
                possibleIndixes[i] = i;
            }

            /*Maximum is 32 for [0, 8192] elements*/
            int verifyLastLists = (int)Math.Floor(((double)(endIndex - startIndex) / elementsPerPermutation) - 1);
            
            Dictionary<int, int[]> result = new Dictionary<int, int[]>();

            for (int i = 0; i < totalCount /*100*/; i++)
            {
                int[] possiblePermutation = new int[elementsPerPermutation];
                for (int j = 0; j < InitialRandomShuffling; j++)
                {
                    RandomShuffleInPlace(possibleIndixes);
                }

                for (int j = 0; j < elementsPerPermutation /*255*/; j++)
                {
                    int index = random.Next(0, endIndex - startIndex); /*Get a random index in the shuffled array of indexes*/
                    possiblePermutation[j] = possibleIndixes[index]; /*Get the index for the permutation*/
                }

                if (possibleIndixes.Length > possiblePermutation.Length)
                {
                    int sIndex = result.Count - verifyLastLists < 0 ? 0 : result.Count - verifyLastLists;
                    int eIndex = (sIndex + verifyLastLists) > result.Count ? result.Count : sIndex + verifyLastLists;

                    /*Check if last verifyLastLists [31] permutations do not contain the same elements*/
                    List<int> allItems = new List<int>();
                    for (int j = sIndex; j < eIndex; j++)
                    {
                        int[] listToControl = result[j];
                        allItems.AddRange(listToControl);
                    }

                    for (int k = 0; k < possiblePermutation.Length; k++)
                    {
                        if (allItems.Contains(possiblePermutation[k]))
                        {
                            possiblePermutation[k--] = random.Next(startIndex, endIndex);
                        }
                        else
                        {
                            allItems.Add(possiblePermutation[k]);
                        }
                    }
                }

                result.Add(i, possiblePermutation);
            }

            return result;
        }

        /// <summary>
        ///   Get random permutations
        /// </summary>
        /// <param name = "totalCount">Number of permutations that are needed for the generation</param>
        /// <param name = "elementsPerPermutation">Elements per permutation</param>
        /// <param name = "startIndex">Start index</param>
        /// <param name = "endIndex">End index</param>
        /// <returns>Permutation pool</returns>
        public Dictionary<int, int[]> GetRandomPermutations(int totalCount, int elementsPerPermutation, int startIndex, int endIndex)
        {
            const int InitialRandomShuffling = 100; /*Number of shuffling*/
            int[] possibleIndixes = new int[endIndex - startIndex]; /*Array of possible indixes*/

            /*Possible indixes will run from 0 - EndIndex [E.g. [0 - 8191]]*/
            for (int i = 0; i < endIndex - startIndex; i++) 
            {
                possibleIndixes[i] = i;
            }

            Dictionary<int, int[]> randomperms = new Dictionary<int, int[]>();
            /*Generating random numbers*/
            for (int i = 0; i < totalCount; i++)
            {
                int[] possiblePermutation = new int[elementsPerPermutation];
                for (int j = 0; j < InitialRandomShuffling; j++)
                {
                    RandomShuffleInPlace(possibleIndixes); /*Shuffle the indixes for 100 times*/
                }

                for (int j = 0; j < elementsPerPermutation /*255*/; j++)
                {
                    int index = Random.Next(0, endIndex - startIndex); /*Get a random index in the shuffled array of indexes*/
                    possiblePermutation[j] = possibleIndixes[index]; /*Get the index for the permutation*/
                }

                randomperms.Add(i, possiblePermutation);
            }

            return randomperms;
        }

        /// <summary>
        /// Random shuffle of the elements in place
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <typeparam name="T">
        /// T type of the elements in the enumeration
        /// </typeparam>
        public void RandomShuffleInPlace<T>(T[] array)
        {
            for (int i = 0, c = array.Length; i < c; i++)
            {
                int index = Random.Next(0, c - 1);
                T a = array[i];
                T b = array[index];
                array[i] = b;
                array[index] = a;
            }
        }
    }
}