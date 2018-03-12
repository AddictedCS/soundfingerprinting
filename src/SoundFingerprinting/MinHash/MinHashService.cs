namespace SoundFingerprinting.MinHash
{
    using System;

    using SoundFingerprinting.Utils;

    internal class MinHashService : IMinHashService
    {
        private readonly IPermutations permutations;

        internal MinHashService(IPermutations permutations)
        {
            this.permutations = permutations;
        }

        public int PermutationsCount
        {
            get
            {
                return permutations.GetPermutations().Length;
            }
        }

        public byte[] Hash(IEncodedFingerprintSchema fingerprint, int n)
        {
            return ComputeMinHashSignature(fingerprint, n);
        }

        /// <summary>
        /// Compute Min Hash signature of a fingerprint
        /// </summary>
        /// <param name="fingerprint">
        /// Fingerprint signature
        /// </param>
        /// <param name="n">
        /// The number of hash functions to use
        /// </param>
        /// <returns>
        /// N-sized sub-fingerprint (length of the permutations number)
        /// </returns>
        /// <remarks>
        /// The basic idea in the Min Hashing scheme is to randomly permute the rows and for each 
        /// column c(i) compute its hash value h(c(i)) as the index of the first row under the permutation that has a 1 in that column.
        /// I.e. http://infolab.stanford.edu/~ullman/mmds/book.pdf s.3.3.4
        /// </remarks>
        private byte[] ComputeMinHashSignature(IEncodedFingerprintSchema fingerprint, int n)
        {
            if (n > PermutationsCount)
            {
                throw new ArgumentException("n should not exceed number of available hash functions: " + PermutationsCount);
            }

            int[][] perms = permutations.GetPermutations();
            byte[] minHash = new byte[n]; /*100*/
            for (int i = 0; i < n; i++)
            {
                minHash[i] = 255; /*The probability of occurrence of 1 after position 255 is very insignificant*/
                for (byte j = 0; j < perms[i].Length /*256*/; j++)
                {
                    int indexAt = perms[i][j];
                    if (fingerprint.IsTrueAt(indexAt))
                    {
                        minHash[i] = j; /*Looking for first occurrence of '1'*/
                        break;
                    }
                }
            }

            return minHash; /*Array of 100 elements with bit turned ON if permutation captured successfully a TRUE bit*/
        }
    }
}