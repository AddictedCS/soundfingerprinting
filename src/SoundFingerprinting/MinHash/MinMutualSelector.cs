namespace SoundFingerprinting.MinHash
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using SoundFingerprinting.Math;

    public abstract class MinMutualSelector : IMinMutualSelector
    {
        protected const double Epsilon = 0.0001;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")][SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public Dictionary<int, List<int[]>> GetPermutations(
            Dictionary<int, int[]> randomPermutationPool, int lHashTable, int bKeysPerTable)
        {
            List<int> possibleIndexes = randomPermutationPool.Keys.ToList(); /*[0, poolcount]*/

            /*Find the unconditional entropy for every permutation*/
            Dictionary<int, double> entropy = new Dictionary<int, double>();
            foreach (KeyValuePair<int, int[]> pair in randomPermutationPool)
            {
                double ent = MathUtility.CalculateEntropy(pair.Value);
                entropy.Add(pair.Key, ent);
            }

            /*Order the permutations in order to find highest unconditional entropy permutations*/
            IOrderedEnumerable<KeyValuePair<int, double>> orderedEntropy = entropy.OrderByDescending(pair => pair.Value);

            /*For each of the L groups assign initial permutation*/
            Dictionary<int, List<int[]>> lGroups = new Dictionary<int, List<int[]>>();
            int count = 0;

            foreach (KeyValuePair<int, double> ordered in orderedEntropy)
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

            return this.ExtractLGroups(randomPermutationPool, bKeysPerTable, possibleIndexes, lGroups);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        protected abstract Dictionary<int, List<int[]>> ExtractLGroups(
            Dictionary<int, int[]> randomPermutationPool,
            int bKeysPerTable,
            List<int> possibleIndexes,
            Dictionary<int, List<int[]>> lGroups);
    }
}