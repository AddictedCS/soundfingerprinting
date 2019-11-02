namespace SoundFingerprinting.MinHash
{
    using System;
    using SoundFingerprinting.Utils;

    internal class ExtendedMinHashService : IMinHashService<int>
    {
        private readonly IPermutations permutations;

        public ExtendedMinHashService(IPermutations permutations)
        {
            this.permutations = permutations;
        }

        public int PermutationsCount => permutations.Count;

        public int IndexesPerPermutation => permutations.IndexesPerPermutation;

        public int[] Hash(IEncodedFingerprintSchema schema, int n)
        {
            if (n > PermutationsCount)
            {
                throw new ArgumentException("n should not exceed number of available hash functions: " + PermutationsCount);
            }

            int[][] perms = permutations.GetPermutations();
            int[] minHash = new int[n];
            for (int i = 0; i < n; i++)
            {
                minHash[i] = perms[i].Length;
                for (int j = 0; j < perms[i].Length; j++)
                {
                    int indexAt = perms[i][j];
                    if (schema.IsTrueAt(indexAt))
                    {
                        minHash[i] = j;
                        break;
                    }
                }
            }

            return minHash;
        }
    }
}
