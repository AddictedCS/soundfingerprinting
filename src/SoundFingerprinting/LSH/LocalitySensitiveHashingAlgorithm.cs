namespace SoundFingerprinting.LSH
{
    using System;
    using System.Diagnostics;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MinHash;

    internal class LocalitySensitiveHashingAlgorithm : ILocalitySensitiveHashingAlgorithm
    {
        private const int MaxNumberOfItemsPerKey = 8; /*Int64 biggest value for MinHash*/

        private readonly IMinHashService minHashService;

        public LocalitySensitiveHashingAlgorithm() : this(DependencyResolver.Current.Get<IMinHashService>())
        {
        }

        internal LocalitySensitiveHashingAlgorithm(IMinHashService minHashService)
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
            if (numberOfHashesPerTable % 2 != 0)
            {
                Trace.WriteLine(
                    "Number of min hash values per table is not equal to power of 2. Expect performance penalty", "Warning");
                return NonPowerOfTwoGroupIntoHashBucket(minHashes, numberOfHashTables, numberOfHashesPerTable);
            }

            return PowerOfTwoGroupIntoHashBucket(minHashes, numberOfHashTables, numberOfHashesPerTable);
        }

        private long[] PowerOfTwoGroupIntoHashBucket(
            byte[] minHashes, int numberOfHashTables, int numberOfHashesPerTable)
        {
            long[] hashBuckets = new long[numberOfHashTables];

            for (int i = 0; i < numberOfHashTables; i++)
            {
                if (numberOfHashesPerTable == 2)
                {
                    hashBuckets[i] = BitConverter.ToInt16(minHashes, i * numberOfHashesPerTable);
                }
                else if (numberOfHashesPerTable == 4)
                {
                    hashBuckets[i] = BitConverter.ToInt32(minHashes, i * numberOfHashesPerTable);
                }
                else
                {
                    hashBuckets[i] = BitConverter.ToInt64(minHashes, i * numberOfHashesPerTable);
                }
            }

            return hashBuckets;
        }

        private long[] NonPowerOfTwoGroupIntoHashBucket(byte[] minHashes, int numberOfHashTables, int numberOfHashesPerTable)
        {
            long[] hashBuckets = new long[numberOfHashTables];
            byte[] array = new byte[MaxNumberOfItemsPerKey];
            for (int i = 0; i < numberOfHashTables; i++)
            {
                Array.Copy(minHashes, i * numberOfHashesPerTable, array, 0, numberOfHashesPerTable);
                hashBuckets[i] = BitConverter.ToInt64(array, 0);
            }

            return hashBuckets;
        }
    }
}
