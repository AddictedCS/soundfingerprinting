namespace SoundFingerprinting.Hashing.MinHash
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using SoundFingerprinting.Hashing.Utils;

    /// <summary>
    ///   Conservative selection of L groups of permutations according to the minimal mutual information spread accross the elements of the group
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class ConservativeSelector : MinMutualSelector
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        protected override Dictionary<int, List<int[]>> ExtractLGroups(
            Dictionary<int, int[]> randomPermutationPool,
            int bKeysPerTable,
            List<int> possibleIndexes,
            Dictionary<int, List<int[]>> lGroups)
        {
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
                        if (@group.Value.Count >= bKeysPerTable)
                        {
                            groupcount++;
                            continue;
                        }

                        double maxMutualInfo = double.MinValue; /*Find the Maximum accross the Group*/
                        foreach (int[] groupMember in @group.Value)
                        {
                            double mi = SignalUtils.MutualInformation(randomPermutationPool[permutationIndex], groupMember);
                            /*Find the maximum of a Group G*/
                            if (maxMutualInfo < mi)
                            {
                                maxMutualInfo = mi;
                            }
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