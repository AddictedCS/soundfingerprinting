namespace SoundFingerprinting.MinHash
{
    using System;
    using System.Linq;

    internal class AdaptivePermutations : IPermutations
    {
        private readonly int[][] permutations;

        private readonly Random random;

        public AdaptivePermutations(int n, int width, int height, int only)
        {
            random = new Random(width * height);
            permutations = new int[n][];
            for (int i = 0; i < n; ++i)
            {
                int count = width * height * 2;
                int[] p = Enumerable.Range(0, count).ToArray();
                RandomShuffleInPlace(p);
                permutations[i] = p.Take(only).ToArray();
            }
        }

        public int[][] GetPermutations()
        {
            return permutations;
        }

        public int Count
        {
            get
            {
                return permutations.Length;
            }
        }

        public int IndexesPerPermutation
        {
            get
            {
                return permutations.First().Length;
            }
        }

        /// <summary>
        /// Random shuffle of the elements in place
        /// i.e. https://en.wikipedia.org/wiki/Fisher–Yates_shuffle
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        private void RandomShuffleInPlace(int[] array)
        {
            for (int i = 0; i < array.Length - 2; i++)
            {
                int j = random.Next(i, array.Length); // i <= j < n
                Swap(array, i, j);
            }
        }

        private static void Swap(int[] array, int i, int j)
        {
            int tmp = array[i];
            array[i] = array[j];
            array[j] = tmp;
        }
    }
}
