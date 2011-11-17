// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.NeuralHashing.MMI
{
    [Serializable]
    public class MinimalMutualInfoPair
    {
        private readonly int _index1St;
        private readonly int _index2Nd;
        private readonly double _mutualInf;

        public MinimalMutualInfoPair(int index1, int index2, double mutualInf)
        {
            _index1St = index1;
            _index2Nd = index2;
            _mutualInf = mutualInf;
        }

        public int IndexFirst
        {
            get { return _index1St; }
        }

        public int IndexSecond
        {
            get { return _index2Nd; }
        }

        public double MutualInformation
        {
            get { return _mutualInf; }
        }
    }
}