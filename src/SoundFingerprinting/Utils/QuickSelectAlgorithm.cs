namespace SoundFingerprinting.Utils
{
    internal class QuickSelectAlgorithm
    {
        public static int Find(int kth, float[] list, ushort[] indexes, int lo, int hi)
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

        private static int Partition(float[] list, ushort[] indexes, int pivotIndex, int lo, int hi)
        {
            float pivotValue = System.Math.Abs(list[pivotIndex]);
            Swap(list, indexes, pivotIndex, hi);
            int storeIndex = lo;
            for (int i = lo; i < hi; ++i)
            {
                if (System.Math.Abs(list[i]) > pivotValue)
                {
                    Swap(list, indexes, storeIndex, i);
                    storeIndex++;
                }
            }

            Swap(list, indexes, hi, storeIndex);
            return storeIndex;
        }

        private static void Swap(float[] list, ushort[] indexes, int i, int j)
        {
            (list[i], list[j]) = (list[j], list[i]);
            (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
        }

        private static void SwapIfGreater(float[] list, ushort[] indexes, int a, int b)
        {
            if (a != b)
            {
                if (System.Math.Abs(list[a]) > System.Math.Abs(list[b]))
                {
                    Swap(list, indexes, a, b);
                }
            }
        }
    }
}
