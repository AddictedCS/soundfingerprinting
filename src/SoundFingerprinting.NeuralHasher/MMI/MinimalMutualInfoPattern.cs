namespace SoundFingerprinting.NeuralHasher.MMI
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using SoundFingerprinting.Hashing.Utils;

    [Serializable]
    public class MinimalMutualInfoPattern
    {
        [NonSerialized] private readonly List<int> indexList = new List<int>();
        private readonly MinimalMutualInfoGroup[] minimalMutualInfoGroups;
        [NonSerialized] private readonly List<MinimalMutualInfoPair> mmiPairs = new List<MinimalMutualInfoPair>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MinimalMutualInfoPattern"/> class. 
        /// </summary>
        /// <param name="numberOfGroups">
        /// Number of groups to create [18]
        /// </param>
        /// <param name="sizeOfGroup">
        /// Size of group [22]
        /// </param>
        public MinimalMutualInfoPattern(int numberOfGroups, int sizeOfGroup)
        {
            NumberOfGroups = numberOfGroups;
            minimalMutualInfoGroups = new MinimalMutualInfoGroup[NumberOfGroups];
            for (int i = 0; i < NumberOfGroups; i++)
            {
                minimalMutualInfoGroups[i] = new MinimalMutualInfoGroup(sizeOfGroup);
            }
        }

        /// <summary>
        ///   Gets the number of groups that the pattern contains
        /// </summary>
        public int NumberOfGroups { get; private set; }

        /// <summary>
        ///   Gets the output dimensionality of the network ensemble
        /// </summary>
        public int NeuralNetworkEnsembleDimensionality { get; private set; }

        /// <summary>
        ///   The group of the pattern on the specified index
        /// </summary>
        /// <param name = "index">Index of the group</param>
        /// <returns>The group on index</returns>
        public MinimalMutualInfoGroup this[int index]
        {
            get { return minimalMutualInfoGroups[index]; }
        }

        /// <summary>
        ///   Creates the hash pattern according to the number of groups specified and their size
        /// </summary>
        /// <param name = "outputs">The data on which mutual information is computed</param>
        public void CreatePattern(double[][] outputs)
        {
            if (outputs.Length * outputs[0].Length < NumberOfGroups * minimalMutualInfoGroups[0].Count)
            {
                throw new ArgumentException("Not enough outputs in order to create a pattern");
            }

            NeuralNetworkEnsembleDimensionality = outputs[0].Length; /*400*/

            // Create a list of indixes
            for (int i = 0; i < outputs[0].Length /*400*/; i++)
            {
                indexList.Add(i);
            }

            ComputePairs(outputs); /*Compute MI between all pairs*/
            InitializeGroups();
            FillGroupsConservative();
        }

        /// <summary>
        ///   Initializes the mmi groups with starting elements that have the highest unconditional entropy
        /// </summary>
        protected void InitializeGroups()
        {
            // Clear all groups
            for (int i = 0; i < minimalMutualInfoGroups.Length; i++)
            {
                minimalMutualInfoGroups[i].Clear();
            }

            // Compute unconditional entropy
            KeyValuePair<int, double>[] uncodEntropy = new KeyValuePair<int, double>[NeuralNetworkEnsembleDimensionality];
            for (int i = 0; i < NeuralNetworkEnsembleDimensionality; i++)
            {
                double uncodEntrpy = 0.0;
                List<MinimalMutualInfoPair> list = mmiPairs.FindAll(pair => pair.IndexFirst == i || pair.IndexSecond == i);
                foreach (MinimalMutualInfoPair pair in list)
                {
                    uncodEntrpy += pair.MutualInformation;
                }

                uncodEntropy[i] = new KeyValuePair<int, double>(i, uncodEntrpy);
            }

            List<KeyValuePair<int, double>> query = new List<KeyValuePair<int, double>>(uncodEntropy.OrderBy(kv => kv.Value));

            // Assign elements that have highest unconditional entropy 
            for (int i = 0; i < minimalMutualInfoGroups.Length; i++)
            {
                minimalMutualInfoGroups[i].AddToGroup(query[i].Key);
                indexList.Remove(query[i].Key);
            }
        }

        /// <summary>
        ///  The most agressive of the methods min ( min (MI(s,t)))
        ///  where s is a permutation and t is a member of the group
        /// </summary>
        protected void FillGroupsAgressive()
        {
            for (int i = 0; i < minimalMutualInfoGroups.Length; i++)
            {
                if (minimalMutualInfoGroups[i].Count != 1)
                {
                    throw new ConstraintException("The groups shouldn't be full or uninitialized");
                }
            }

            while (true)
            {
                MinimalMutualInfoPair minMi = new MinimalMutualInfoPair(-1, -1, double.MaxValue);
                int groupIndexToAdd = -1;
                int uniqueIndex = -1;
                for (int i = 0; i < indexList.Count; i++)
                {
                    for (int j = 0; j < minimalMutualInfoGroups.Length; j++)
                    {
                        if (minimalMutualInfoGroups[j].Count == minimalMutualInfoGroups[j].GroupSize)
                        {
                            continue;
                        }

                        for (int k = 0; k < minimalMutualInfoGroups[j].Count; k++)
                        {
                            // Find mi between selected index and current member of the group
                            MinimalMutualInfoPair mi = mmiPairs.Find(
                                pair => (pair.IndexFirst == indexList[i] && pair.IndexSecond == minimalMutualInfoGroups[j][k]) ||
                                        (pair.IndexFirst == minimalMutualInfoGroups[j][k] && pair.IndexSecond == indexList[i]));
                            if (minMi.MutualInformation > mi.MutualInformation)
                            {
                                minMi = mi;
                                groupIndexToAdd = j;
                                uniqueIndex = minimalMutualInfoGroups[j].Contains(minMi.IndexFirst) ? minMi.IndexSecond : minMi.IndexFirst;
                            }
                        }
                    }
                }

                if (minMi.IndexFirst == -1)
                {
                    break;
                }

                indexList.Remove(uniqueIndex);
                minimalMutualInfoGroups[groupIndexToAdd].AddToGroup(uniqueIndex);
            }
        }

        /// <summary>
        ///   The most conservative method due to the fact that it minimizes the worst of correlations
        ///   min ( max (MI(s,t))) 
        ///   where s is a permutation and t is a member of the group
        ///   Assign  MI to a group to be the maximum of the MIs between s and any member of
        ///   this group. Select the minimum of these across groups and unselected indeces
        /// </summary>
        protected void FillGroupsConservative()
        {
            for (int i = 0; i < minimalMutualInfoGroups.Length; i++)
            {
                if (minimalMutualInfoGroups[i].Count != 1)
                {
                    throw new ConstraintException("The groups shouldn't be full or uninitialized");
                }
            }

            while (true)
            {
                double minMi = double.MaxValue;
                int groupIndexToAdd = -1;
                int uniqueIndex = -1;
                bool finished = true;
                for (int i = 0; i < indexList.Count; i++)
                {
                    for (int j = 0; j < minimalMutualInfoGroups.Length; j++)
                    {
                        double groupMax = 0.0;
                        if (minimalMutualInfoGroups[j].Count == minimalMutualInfoGroups[j].GroupSize)
                        {
                            continue;
                        }

                        finished = false;
                        for (int k = 0; k < minimalMutualInfoGroups[j].Count; k++)
                        {
                            // Find mi between selected index and current member of the group
                            MinimalMutualInfoPair mi = mmiPairs.Find(
                                pair => (pair.IndexFirst == indexList[i] && pair.IndexSecond == minimalMutualInfoGroups[j][k]) ||
                                        (pair.IndexFirst == minimalMutualInfoGroups[j][k] && pair.IndexSecond == indexList[i]));
                            if (mi.MutualInformation > groupMax)
                            {
                                groupMax = mi.MutualInformation;
                            }
                        }

                        if (minMi > groupMax)
                        {
                            minMi = groupMax;
                            groupIndexToAdd = j;
                            uniqueIndex = indexList[i];
                        }
                    }
                }

                if (finished)
                {
                    break;
                }

                indexList.Remove(uniqueIndex);
                minimalMutualInfoGroups[groupIndexToAdd].AddToGroup(uniqueIndex);
            }
        }

        /// <summary>
        ///   Computes mutual information between all pairs
        /// </summary>
        /// <param name = "outputs">Actual data on which MI is computed</param>
        private void ComputePairs(double[][] outputs)
        {
            int netOutputsCount = outputs[0].Length;

            double[] samples1 = new double[outputs.Length];
            double[] samples2 = new double[outputs.Length];

            for (int i = 0; i < netOutputsCount - 1 /*400*/; i++)
            {
                for (int j = i + 1; j < netOutputsCount; j++)
                {
                    // Copy to arrays the contents of particular i and j
                    for (int k = 0, n = outputs.Length; k < n; k++)
                    {
                        samples1[k] = outputs[k][i];
                        samples2[k] = outputs[k][j];
                    }

                    double mutualInf = SignalUtils.MutualInformation(samples1, samples2);
                    mmiPairs.Add(new MinimalMutualInfoPair(i, j, mutualInf));
                }
            }
        }
    }
}