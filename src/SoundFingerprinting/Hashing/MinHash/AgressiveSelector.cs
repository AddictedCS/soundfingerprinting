namespace SoundFingerprinting.Hashing.MinHash
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using SoundFingerprinting.Hashing.Utils;

    /// <summary>
    ///   Aggressive selector of LGroup of permutation
    /// </summary>
    public class AgressiveSelector : MinMutualSelector
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        protected override Dictionary<int, List<int[]>> ExtractLGroups(Dictionary<int, int[]> randomPermutationPool, int bKeysPerTable, List<int> possibleIndexes, Dictionary<int, List<int[]>> lGroups)
        {
            /*Agresive selection*/
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

                        foreach (int[] groupMember in group.Value)
                        {
                            /*Actual agressive selection*/
                            double mi = SignalUtils.MutualInformation(randomPermutationPool[permutationIndex], groupMember);
                            if (minMutualInfo > mi)
                            {
                                minMutualInfo = mi;
                                permIndex = permutationIndex;
                                lGroupIndex = groupcount;
                            }
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