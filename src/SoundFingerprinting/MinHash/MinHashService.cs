namespace SoundFingerprinting.MinHash
{
    using SoundFingerprinting.Infrastructure;

    public class MinHashService : IMinHashService
    {
        private readonly IPermutations permutations;

        public MinHashService()
            : this(DependencyResolver.Current.Get<IPermutations>())
        {
        }

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

        public byte[] Hash(bool[] fingerprint)
        {
            return ComputeMinHashSignature(fingerprint);
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
            int[][] perms = permutations.GetPermutations();
            byte[] minHash = new byte[perms.Length]; /*100*/
            for (int i = 0; i < perms.Length /*100*/; i++)
            {
                minHash[i] = 255; /*The probability of occurrence of 1 after position 255 is very insignificant*/
                for (int j = 0; j < perms[i].Length /*256*/; j++)
                {
                    if (signature[perms[i][j]])
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