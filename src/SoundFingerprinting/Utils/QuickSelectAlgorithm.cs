namespace SoundFingerprinting.Utils
{
    using System;

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

        /// <summary>
        ///  Place the k-th smallest value of <paramref name="values"/> at index <paramref name="k"/>.
        ///  After return, every element before index <paramref name="k"/> is less than or equal to <c>values[k]</c>,
        ///  and every element after is greater than or equal to <c>values[k]</c>.
        ///  Average O(n), worst-case O(n²). Mutates <paramref name="values"/> in place.
        /// </summary>
        /// <param name="values">Values to partition. Modified in place.</param>
        /// <param name="k">Zero-based rank to select.</param>
        /// <returns>The value at the k-th position in sorted order.</returns>
        public static double SelectNthSmallest(double[] values, int k)
        {
            return SelectNthSmallest(values.AsSpan(), k);
        }

        /// <summary>
        ///  Span-typed overload of <see cref="SelectNthSmallest(double[],int)"/>; lets callers operate on a
        ///  prefix of a pooled or reused buffer without an extra array allocation.
        /// </summary>
        /// <param name="values">Span to partition. Modified in place.</param>
        /// <param name="k">Zero-based rank to select.</param>
        /// <returns>The value at the k-th position in sorted order.</returns>
        public static double SelectNthSmallest(Span<double> values, int k)
        {
            int lo = 0;
            int hi = values.Length - 1;
            while (lo < hi)
            {
                int mid = lo + ((hi - lo) / 2);
                SwapIfGreater(values, lo, mid);
                SwapIfGreater(values, lo, hi);
                SwapIfGreater(values, mid, hi);
                int pivot = PartitionAscending(values, mid, lo, hi);
                if (pivot == k)
                {
                    return values[pivot];
                }

                if (pivot > k)
                {
                    hi = pivot - 1;
                }
                else
                {
                    lo = pivot + 1;
                }
            }

            return values[lo];
        }

        private static int PartitionAscending(Span<double> values, int pivotIndex, int lo, int hi)
        {
            double pivotValue = values[pivotIndex];
            SwapDouble(values, pivotIndex, hi);
            int storeIndex = lo;
            for (int i = lo; i < hi; ++i)
            {
                if (values[i] < pivotValue)
                {
                    SwapDouble(values, storeIndex, i);
                    storeIndex++;
                }
            }

            SwapDouble(values, hi, storeIndex);
            return storeIndex;
        }

        private static void SwapDouble(Span<double> values, int i, int j)
        {
            (values[i], values[j]) = (values[j], values[i]);
        }

        private static void SwapIfGreater(Span<double> values, int a, int b)
        {
            if (a != b && values[a] > values[b])
            {
                SwapDouble(values, a, b);
            }
        }

        private static int Partition(float[] list, ushort[] indexes, int pivotIndex, int lo, int hi)
        {
            float pivotValue = Math.Abs(list[pivotIndex]);
            Swap(list, indexes, pivotIndex, hi);
            int storeIndex = lo;
            for (int i = lo; i < hi; ++i)
            {
                if (Math.Abs(list[i]) > pivotValue)
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
                if (Math.Abs(list[a]) > Math.Abs(list[b]))
                {
                    Swap(list, indexes, a, b);
                }
            }
        }
    }
}
