namespace Soundfingerprinting.Hashing.LSH
{
    using System;

    public class LSHService : ILSHService
    {
        private const int MaxNumberOfItemsPerKey = 8; /*Int64 biggest value for MinHash*/

        public long[] Hash(byte[] source, int numberOfHashTables, int numberOfHashesPerTable)
        {
            return GroupHashesToLSHBuckets(source, numberOfHashTables, numberOfHashesPerTable);
        }

        /// <summary>
        ///   Compute LSH hash buckets which will be inserted into hash tables.
        ///   Each fingerprint will have a candidate in each of the hash tables.
        /// </summary>
        /// <param name = "minHashes">Min Hashes gathered from every fingerprint [N = 100]</param>
        /// <param name = "numberOfHashTables">Number of hash tables [L = 25]</param>
        /// <param name = "numberOfHashesPerTable">Number of min hashes per key [N = 4]</param>
        /// <returns>Collection of Pairs with Key = Hash table index, Value = Hash bucket</returns>
        private long[] GroupHashesToLSHBuckets(byte[] minHashes, int numberOfHashTables, int numberOfHashesPerTable)
        {
            long[] result = new long[numberOfHashTables];

            for (int i = 0; i < numberOfHashTables /*hash functions*/; i++)
            {
                byte[] array = new byte[MaxNumberOfItemsPerKey];
                for (int j = 0; j < numberOfHashesPerTable /*r min hash signatures*/; j++)
                {
                    array[j] = minHashes[(i * numberOfHashesPerTable) + j];
                }

                long hashbucket = BitConverter.ToInt64(array, 0); // actual value of the signature
                result[i] = hashbucket;
            }

            return result;
        }
    }
}
