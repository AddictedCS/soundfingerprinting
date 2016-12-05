namespace SoundFingerprinting.Utils
{
    using System;
    using System.Linq;

    internal class FastFingerprintDescriptor : FingerprintDescriptor
    {
        public override bool[] ExtractTopWavelets(float[][] frames, int topWavelets)
        {
            float[] concatenated = ConcatenateFrames(frames);
            int[] indexes = Enumerable.Range(0, concatenated.Length).ToArray();
            int pi = Find(topWavelets, concatenated, indexes, 0, concatenated.Length - 1);
            Partition(concatenated, indexes, pi, concatenated.Length - 1);
            bool[] result = EncodeFingerprint(concatenated, indexes, topWavelets);
            return result;
        }

        private int Find(int kth, float[] list, int[] indexes, int lo, int hi)
        {
            int pi = Partition(list, indexes, lo, hi);
            if (pi == list.Length - kth)
            {
                return pi;
            }

            if (pi > list.Length - kth)
            {
                return Find(kth, list, indexes, lo, pi - 1);
            }

            return Find(kth, list, indexes, pi + 1, hi);
        }

        private int Partition(float[] list, int[] indexes, int lo, int hi)
        {
            float pivot = list[lo];

            int i = lo + 1, j = lo + 1;
            for (; j <= hi; j++)
            {
                if (Math.Abs(list[j]).CompareTo(pivot) > 0)
                {
                    Swap(list, indexes, i, j);
                    i++;
                }
            }

            Swap(list, indexes, lo, i - 1);
            return i - 1;
        }

        private void Swap(float[] list, int[] indexes, int i, int j)
        {
            float tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;

            int indexTmp = indexes[i];
            indexes[i] = indexes[j];
            indexes[j] = indexTmp;
        }
    }
}
