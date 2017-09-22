namespace SoundFingerprinting.Utils
{
    using System;

    public class QuickSelectAlgorithm
    {
        public int Find(int kth, float[] list, int[] indexes, int lo, int hi)
        {
            while (lo != hi)
            {
                int mid = lo + ((hi - lo) / 2);
                SwapIfGreater(list, indexes, lo, mid);
                SwapIfGreater(list, indexes, lo, hi);
                SwapIfGreater(list, indexes, mid, hi);
                int pi = mid;
                pi = Partition(list, indexes, pi, lo, hi);
                if (pi == kth)
                {
                    return pi;
                }

                if (pi > kth)
                {
                    hi = pi - 1;
                }
                else
                {
                    lo = pi + 1;
                }
            }

            return lo;
        }

        private int Partition(float[] list, int[] indexes, int pivotIndex, int lo, int hi)
        {
            float pivotValue = Math.Abs(list[pivotIndex]);
            Swap(list, indexes, pivotIndex, hi);
            int storeIndex = lo;
            for (int i = lo; i < hi; ++i)
            {
                if (Math.Abs(list[i]).CompareTo(pivotValue) > 0)
                {
                    Swap(list, indexes, storeIndex, i);
                    storeIndex++;
                }
            }

            Swap(list, indexes, hi, storeIndex);
            return storeIndex;
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

        private void SwapIfGreater(float[] list, int[] indexes, int a, int b)
        {
            if (a != b)
            {
                if (Math.Abs(list[a]).CompareTo(Math.Abs(list[b])) > 0)
                {
                    this.Swap(list, indexes, a, b);
                }
            }
        }
    }
}
