namespace SoundFingerprinting.NeuralHasher.MMI
{
    using System;

    [Serializable]
    public class MinimalMutualInfoPair
    {
        private readonly int index1St;
        private readonly int index2Nd;
        private readonly double mutualInf;

        public MinimalMutualInfoPair(int index1, int index2, double mutualInf)
        {
            index1St = index1;
            index2Nd = index2;
            this.mutualInf = mutualInf;
        }

        public int IndexFirst
        {
            get { return index1St; }
        }

        public int IndexSecond
        {
            get { return index2Nd; }
        }

        public double MutualInformation
        {
            get { return mutualInf; }
        }
    }
}