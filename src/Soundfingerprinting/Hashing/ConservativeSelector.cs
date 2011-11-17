// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soundfingerprinting.Hashing
{
    /// <summary>
    ///   Conservative selection of LGroups of permutations according to the minimal mutual information 
    ///   spread accross the elements of the group
    /// </summary>
    public class ConservativeSelector : IMinMutualSelector
    {
        #region IMinMutualSelector Members

        public Dictionary<int, List<int[]>> GetPermutations(Dictionary<int, int[]> randomPermutationPool, int lHashTable, int bKeysPerTable)
        {
            List<int> possibleIndexes = randomPermutationPool.Keys.ToList(); /*[0, poolcount]*/

            /*Find the unconditional entropy for every permutation*/
            Dictionary<int, double> entropy = new Dictionary<int, double>();
            foreach (KeyValuePair<int, int[]> pair in randomPermutationPool)
            {
                double ent = SignalUtils.Entropy(pair.Value);
                entropy.Add(pair.Key, ent);
            }

            /*Order the permutations in order to find highest unconditional entropy permutations*/
            IOrderedEnumerable<KeyValuePair<int, double>> order = entropy.OrderByDescending((pair) => pair.Value);

            /*For each of the L groups assign initial permutation*/
            Dictionary<int, List<int[]>> LGroups = new Dictionary<int, List<int[]>>();
            int count = 0;
            foreach (KeyValuePair<int, double> ordered in order)
            {
                if (count < lHashTable)
                {
                    LGroups.Add(count, new List<int[]>());
                    LGroups[count].Add(randomPermutationPool[ordered.Key]);
                    possibleIndexes.Remove(ordered.Key);
                }
                else
                    break;
                count++;
            }

            /*Conservative selection*/
            while (true)
            {
                double minMutualInfo = Double.MaxValue;
                int permIndex = -1;
                int lGroupIndex = -1;
                foreach (int permutationIndex in possibleIndexes)
                {
                    int groupcount = 0;
                    foreach (KeyValuePair<int, List<int[]>> group in LGroups)
                    {
                        /*Check if there is space in set G of L, for a new permutation added to B Keys*/
                        if (group.Value.Count >= bKeysPerTable)
                        {
                            groupcount++;
                            continue;
                        }
                        double maxMutualInfo = Double.MinValue; /*Find the Maximum accross the Group*/
                        foreach (int[] groupMember in group.Value)
                        {
                            double mi = SignalUtils.MutualInformation(randomPermutationPool[permutationIndex], groupMember);
                            if (maxMutualInfo < mi) /*Find the maximum of a Group G*/
                                maxMutualInfo = mi;
                        }

                        /*Select the minimum of the permutation across all the Groups G and Unselected permutations S*/
                        if (minMutualInfo > maxMutualInfo)
                        {
                            minMutualInfo = maxMutualInfo;
                            permIndex = permutationIndex;
                            lGroupIndex = groupcount;
                        }
                        groupcount++;
                    }
                }
                if (minMutualInfo == Double.MaxValue && permIndex == -1 && lGroupIndex == -1)
                    break;
                LGroups[lGroupIndex].Add(randomPermutationPool[permIndex]);
                possibleIndexes.Remove(permIndex);
            }
            return LGroups;
        }

        #endregion
    }
}