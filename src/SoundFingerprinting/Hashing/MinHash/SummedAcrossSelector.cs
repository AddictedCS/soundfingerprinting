namespace SoundFingerprinting.Hashing.MinHash
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using SoundFingerprinting.Hashing.Utils;

    /// <summary>
    ///   Summed accross selection of LGroups of permutations according to the minimal mutual information 
    ///   spread accross the elements of the group
    /// </summary>
    public class SummedAcrossSelector : IMinMutualSelector
    {
        private const double Epsilon = 0.001;

        /// <summary>
        ///   Get permutations using summed across technique
        /// </summary>
        /// <param name = "randomPermutationPool">Random permutation pool</param>
        /// <param name = "lHashTable">L Hash tables</param>
        /// <param name = "bKeysPerTable">K keys per table</param>
        /// <returns>LGroups of permutation specific to LHashtables</returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public Dictionary<int, List<int[]>> GetPermutations(Dictionary<int, int[]> randomPermutationPool, int lHashTable, int bKeysPerTable)
        {
            List<int> possibleIndexes = randomPermutationPool.Keys.ToList(); /*[0, poolcount]*/

            /*Find the unconditional entropy for every permutation*/
            Dictionary<int, double> entropy = new Dictionary<int, double>();
            foreach (KeyValuePair<int, int[]> pair in randomPermutationPool)
            {
                double ent = SignalUtils.CalculateEntropy(pair.Value);
                entropy.Add(pair.Key, ent);
            }

            /*Order the permutations in order to find highest unconditional entropy permutations*/
            IOrderedEnumerable<KeyValuePair<int, double>> order = entropy.OrderByDescending(pair => pair.Value);

            /*For each of the L groups assign initial permutation*/
            Dictionary<int, List<int[]>> lGroups = new Dictionary<int, List<int[]>>();
            int count = 0;
            foreach (KeyValuePair<int, double> ordered in order)
            {
                if (count < lHashTable)
                {
                    lGroups.Add(count, new List<int[]>());
                    lGroups[count].Add(randomPermutationPool[ordered.Key]);
                    possibleIndexes.Remove(ordered.Key);
                }
                else
                {
                    break;
                }

                count++;
            }

            /*Summed accros selection*/
            while (true)
            {
                double minMutualInfo = double.MaxValue;
                int permIndex = -1;
                int lGroupIndex = -1;
                foreach (int permutationIndex in possibleIndexes)
                {
                    int groupcount = 0;
                    foreach (KeyValuePair<int, List<int[]>> group in lGroups)
                    {
                        /*Check if there is space in set G of L, for a new permutation added to B Keys*/
                        if (group.Value.Count >= bKeysPerTable)
                        {
                            groupcount++;
                            continue;
                        }

                        double totalMinMutualInfo = 0;
                        foreach (int[] groupMember in group.Value)
                        {
                            totalMinMutualInfo += SignalUtils.MutualInformation(randomPermutationPool[permutationIndex], groupMember);
                        }

                        /*Actual summed accross selection*/
                        if (minMutualInfo > totalMinMutualInfo)
                        {
                            minMutualInfo = totalMinMutualInfo;
                            permIndex = permutationIndex;
                            lGroupIndex = groupcount;
                        }

                        groupcount++;
                    }
                }

                if (Math.Abs(minMutualInfo - double.MaxValue) < Epsilon && permIndex == -1 && lGroupIndex == -1)
                {
                    break;
                }

                lGroups[lGroupIndex].Add(randomPermutationPool[permIndex]);
                possibleIndexes.Remove(permIndex);
            }

            return lGroups;
        }
    }
}