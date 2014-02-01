namespace SoundFingerprinting.LSH
{
    public class BoundedLocalitySensitiveHashingAlgorithm : LocalitySensitiveHashingAlgorithm
    {
        /// <summary>
        ///   Maximum number of hash buckets in the database
        /// </summary>
        private const int HashBucketSize = 100000;

        /// <summary>
        ///   The smallest Prime number that exceeds (D / 2)
        ///   <remarks>
        ///     D - domain of values. D = 2^(8*nrKeys)
        ///     5 Keys - 549755813911
        ///     4 Keys - 2147483659
        ///     3 Keys - 8388617
        ///     2 Keys - 32771
        ///   </remarks>
        /// </summary>
        private const long PrimeP = 2147483659;

        /// <summary>
        ///   A Constant used in computation of  hash bucket value
        /// </summary>
        private const int A = 1;

        /// <summary>
        ///   B Constant used in computation of hash bucket value
        /// </summary>
        private const int B = 0;

        protected override long[] GroupIntoHashTables(
            byte[] minHashes, int numberOfHashTables, int numberOfHashesPerTable)
        {
            var hashes = base.GroupIntoHashTables(minHashes, numberOfHashTables, numberOfHashesPerTable);

            for (int i = 0; i < hashes.Length; i++)
            {
                hashes[i] = (((A * hashes[i]) + B) % PrimeP) % HashBucketSize;
            }

            return hashes;
        }
    }
}
