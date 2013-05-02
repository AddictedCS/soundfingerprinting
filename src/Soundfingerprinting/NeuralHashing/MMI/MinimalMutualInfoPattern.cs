// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Soundfingerprinting.Hashing;

namespace Soundfingerprinting.NeuralHashing.MMI
{
    using Soundfingerprinting.Hashing.Utils;

    [Serializable]
    public class MinimalMutualInfoPattern
    {
        [NonSerialized] private readonly List<int> _indecesList = new List<int>();
        private readonly MinimalMutualInfoGroup[] _minimalMutualInfoGroups;
        [NonSerialized] private readonly List<MinimalMutualInfoPair> _mmiPairs = new List<MinimalMutualInfoPair>();
        private readonly int _numberOfGroups;
        private int _neuralNetworkDimensionality;

        #region Propreties

        /// <summary>
        ///   Number of groups that the pattern contains
        /// </summary>
        public int NumberOfGroups
        {
            get { return _numberOfGroups; }
        }

        /// <summary>
        ///   The output dimensionality of the network ensemble
        /// </summary>
        public int NNEnsembleDimensionality
        {
            get { return _neuralNetworkDimensionality; }
        }

        /// <summary>
        ///   The group of the pattern on the specified index
        /// </summary>
        /// <param name = "index">Index of the group</param>
        /// <returns>The group on index</returns>
        public MinimalMutualInfoGroup this[int index]
        {
            get { return _minimalMutualInfoGroups[index]; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Creates Minimal Mutual Information class
        /// </summary>
        /// <param name = "numberOfGroups">Number of groups to create [18]</param>
        /// <param name = "sizeOfGroup">Size of group [22]</param>
        public MinimalMutualInfoPattern(int numberOfGroups, int sizeOfGroup)
        {
            _numberOfGroups = numberOfGroups;
            _minimalMutualInfoGroups = new MinimalMutualInfoGroup[_numberOfGroups];
            for (int i = 0; i < _numberOfGroups; i++)
            {
                _minimalMutualInfoGroups[i] = new MinimalMutualInfoGroup(sizeOfGroup);
            }
        }

        #endregion

        /// <summary>
        ///   Creates the hash pattern according to the number of groups specified and their size
        /// </summary>
        /// <param name = "outputs">The data on which mutual information is computed</param>
        public void CreatePattern(double[][] outputs)
        {
            if (outputs == null || outputs[0] == null)
                throw new ArgumentNullException("outputs");
            if (outputs.Length*outputs[0].Length < _numberOfGroups*_minimalMutualInfoGroups[0].Count)
                throw new ArgumentException("Not enough outputs in order to create a pattern");

            _neuralNetworkDimensionality = outputs[0].Length; /*400*/

            //Create a list of indices
            for (int i = 0; i < outputs[0].Length /*400*/; i++)
            {
                _indecesList.Add(i);
            }

            ComputePairs(outputs); /*Compute MI between all pairs*/
            InitializeGroups(); /**/
            FillGroupsConservative();
        }


        /// <summary>
        ///   Initializes the mmi groups with starting elements that have the highest unconditional entropy
        /// </summary>
        protected void InitializeGroups()
        {
            //Clear all groups
            for (int i = 0; i < _minimalMutualInfoGroups.Length; i++)
            {
                _minimalMutualInfoGroups[i].Clear();
            }

            //Compute unconditional entropy
            KeyValuePair<int, double>[] uncodEntropy = new KeyValuePair<int, double>[_neuralNetworkDimensionality];
            for (int i = 0; i < _neuralNetworkDimensionality; i++)
            {
                double uncodEntrpy = 0.0;
                List<MinimalMutualInfoPair> list = _mmiPairs.FindAll(pair => pair.IndexFirst == i || pair.IndexSecond == i);
                foreach (MinimalMutualInfoPair pair in list)
                {
                    uncodEntrpy += pair.MutualInformation;
                }
                uncodEntropy[i] = new KeyValuePair<int, double>(i, uncodEntrpy);
            }


            List<KeyValuePair<int, double>> query = new List<KeyValuePair<int, double>>(uncodEntropy.OrderBy(kv => kv.Value));

            //Assign elements that have highest unconditional entropy 
            for (int i = 0; i < _minimalMutualInfoGroups.Length; i++)
            {
                _minimalMutualInfoGroups[i].AddToGroup(query[i].Key);
                _indecesList.Remove(query[i].Key);
            }
        }


        ///<summary>
        ///  The most agressive of the methods
        /// 
        ///  min ( min (MI(s,t)))
        /// 
        ///  where s is a permutation and t is a member of the group
        ///</summary>
        protected void FillGroupsAgressive()
        {
            for (int i = 0; i < _minimalMutualInfoGroups.Length; i++)
            {
                if (_minimalMutualInfoGroups[i].Count != 1)
                    throw new ConstraintException("The groups shouldn't be full or uninitialized");
            }

            while (true)
            {
                MinimalMutualInfoPair minMi = new MinimalMutualInfoPair(-1, -1, Double.MaxValue);
                int groupIndexToAdd = -1;
                int uniqueIndex = -1;
                for (int i = 0; i < _indecesList.Count; i++)
                {
                    for (int j = 0; j < _minimalMutualInfoGroups.Length; j++)
                    {
                        if (_minimalMutualInfoGroups[j].Count == _minimalMutualInfoGroups[j].GroupSize)
                            continue;
                        for (int k = 0; k < _minimalMutualInfoGroups[j].Count; k++)
                        {
                            //Find mi between selected index and current member of the group
                            MinimalMutualInfoPair mi = _mmiPairs.Find(
                                pair => (pair.IndexFirst == _indecesList[i] && pair.IndexSecond == _minimalMutualInfoGroups[j][k]) ||
                                        (pair.IndexFirst == _minimalMutualInfoGroups[j][k] && pair.IndexSecond == _indecesList[i]));
                            if (minMi.MutualInformation > mi.MutualInformation)
                            {
                                minMi = mi;
                                groupIndexToAdd = j;
                                if (_minimalMutualInfoGroups[j].Contains(minMi.IndexFirst))
                                    uniqueIndex = minMi.IndexSecond;
                                else
                                    uniqueIndex = minMi.IndexFirst;
                            }
                        }
                    }
                }
                if (minMi.IndexFirst == -1)
                    break;
                _indecesList.Remove(uniqueIndex);
                _minimalMutualInfoGroups[groupIndexToAdd].AddToGroup(uniqueIndex);
            }
        }

        /// <summary>
        ///   The most conservative method due to the fact that it minimizes 
        ///   the worst of correlations
        /// 
        ///   min ( max (MI(s,t))) 
        /// 
        ///   where s is a permutation and t is a member of the group
        ///   Assign  MI to a group to be the maximum of the MIs between s and any member of
        ///   this group. Select the minimum of these across groups and unselected indeces
        /// </summary>
        protected void FillGroupsConservative()
        {
            for (int i = 0; i < _minimalMutualInfoGroups.Length; i++)
            {
                if (_minimalMutualInfoGroups[i].Count != 1)
                    throw new ConstraintException("The groups shouldn't be full or uninitialized");
            }

            while (true)
            {
                double minMi = Double.MaxValue;
                int groupIndexToAdd = -1;
                int uniqueIndex = -1;
                bool finished = true;
                for (int i = 0; i < _indecesList.Count; i++)
                {
                    for (int j = 0; j < _minimalMutualInfoGroups.Length; j++)
                    {
                        double groupMax = 0.0;
                        if (_minimalMutualInfoGroups[j].Count == _minimalMutualInfoGroups[j].GroupSize)
                            continue;
                        finished = false;
                        for (int k = 0; k < _minimalMutualInfoGroups[j].Count; k++)
                        {
                            //Find mi between selected index and current member of the group
                            MinimalMutualInfoPair mi = _mmiPairs.Find(
                                pair => (pair.IndexFirst == _indecesList[i] && pair.IndexSecond == _minimalMutualInfoGroups[j][k]) ||
                                        (pair.IndexFirst == _minimalMutualInfoGroups[j][k] && pair.IndexSecond == _indecesList[i]));
                            if (mi.MutualInformation > groupMax)
                            {
                                groupMax = mi.MutualInformation;
                            }
                        }
                        if (minMi > groupMax)
                        {
                            minMi = groupMax;
                            groupIndexToAdd = j;
                            uniqueIndex = _indecesList[i];
                        }
                    }
                }
                if (finished)
                    break;
                _indecesList.Remove(uniqueIndex);
                _minimalMutualInfoGroups[groupIndexToAdd].AddToGroup(uniqueIndex);
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
                    //Copy to arrays the contents of particular i and j
                    for (int k = 0, n = outputs.Length; k < n; k++)
                    {
                        samples1[k] = outputs[k][i];
                        samples2[k] = outputs[k][j];
                    }

                    double mutualInf = SignalUtils.MutualInformation(samples1, samples2);
                    _mmiPairs.Add(new MinimalMutualInfoPair(i, j, mutualInf));
                }
            }
        }
    }
}