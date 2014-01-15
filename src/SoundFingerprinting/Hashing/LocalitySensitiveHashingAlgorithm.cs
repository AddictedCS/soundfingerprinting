namespace SoundFingerprinting.Hashing
{
    using System;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Infrastructure;

    public class LocalitySensitiveHashingAlgorithm : ILocalitySensitiveHashingAlgorithm
    {
        private const int MaxNumberOfItemsPerKey = 8; /*Int64 biggest value for MinHash*/

        private readonly IMinHashService minHashService;

        public LocalitySensitiveHashingAlgorithm() : this(DependencyResolver.Current.Get<IMinHashService>())
        {
        }

        public LocalitySensitiveHashingAlgorithm(IMinHashService minHashService)
        {
            this.minHashService = minHashService;
        }

        public HashData Hash(bool[] fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable)
        {
            byte[] subFingerprint = minHashService.Hash(fingerprint);
            return new HashData(
                subFingerprint, GroupIntoHashTables(subFingerprint, numberOfHashTables, numberOfHashKeysPerTable));
        }

        /// <summary>
        ///   Compute LSH hash buckets which will be inserted into hash tables.
        ///   Each fingerprint will have a candidate in each of the hash tables.
        /// </summary>
        /// <param name = "minHashes">Min Hashes gathered from every fingerprint [N = 100]</param>
        /// <param name = "numberOfHashTables">Number of hash tables [L = 25]</param>
        /// <param name = "numberOfHashesPerTable">Number of min hashes per key [N = 4]</param>
        /// <returns>Collection of Pairs with Key = Hash table index, Value = Hash bin</returns>
        protected virtual long[] GroupIntoHashTables(byte[] minHashes, int numberOfHashTables, int numberOfHashesPerTable)
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
