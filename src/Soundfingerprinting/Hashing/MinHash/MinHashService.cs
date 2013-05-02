namespace Soundfingerprinting.Hashing.MinHash
{
    using System;

    public class MinHashService : IMinHashService
    {
        private readonly int[][] permutations;

        public MinHashService(IPermutations permutations)
        {
            this.permutations = permutations.GetPermutations(); /*Read the permutation from the undercover*/

            if (this.permutations == null || this.permutations.Length == 0)
            {
                throw new Exception("Permutations are null or not enough to create the Min Hash signature");
            }
        }

        public int PermutationsCount
        {
            get
            {
                return this.permutations.Length;
            }
        }

        public byte[] Hash(bool[] fingerprint)
        {
            return this.ComputeMinHashSignature(fingerprint);
        }

        /// <summary>
        ///   Compute Min Hash signature of a fingerprint
        /// </summary>
        /// <param name = "fingerprint">Fingerprint signature</param>
        /// <returns>P-sized sub-fingerprint (length of the permutations number)</returns>
        /// <remarks>
        ///   The basic idea in the Min Hashing scheme is to randomly permute the rows and for each 
        ///   column c(i) compute its hash value h(c(i)) as the index of the first row under the permutation that has a 1 in that column.
        /// </remarks>
        private byte[] ComputeMinHashSignature(bool[] fingerprint)
        {
            bool[] signature = fingerprint;
            byte[] minHash = new byte[this.permutations.Length]; /*100*/
            for (int i = 0; i < this.permutations.Length /*100*/; i++)
            {
                minHash[i] = 255; /*The probability of occurrence of 1 after position 255 is very insignificant*/
                for (int j = 0; j < this.permutations[i].Length /*256*/; j++)
                {
                    if (signature[this.permutations[i][j]])
                    {
                        minHash[i] = (byte)j; /*Looking for first occurrence of '1'*/
                        break;
                    }
                }
            }

            return minHash; /*Array of 100 elements with bit turned ON if permutation captured successfully a TRUE bit*/
        }
    }
}