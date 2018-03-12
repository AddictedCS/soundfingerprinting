namespace SoundFingerprinting.LSH
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;

    internal class LocalitySensitiveHashingAlgorithm : ILocalitySensitiveHashingAlgorithm
    {
        private const int LargePrime = 433494437;

        private readonly IMinHashService minHashService;
        private readonly IHashConverter hashConverter;

        internal LocalitySensitiveHashingAlgorithm(IMinHashService minHashService, IHashConverter hashConverter)
        {
            this.minHashService = minHashService;
            this.hashConverter = hashConverter;
        }

        public HashedFingerprint Hash(Fingerprint fingerprint, HashingConfig hashingConfig,  IEnumerable<string> clusters)
        {
            int numberOfHashTables = hashingConfig.NumberOfLSHTables;
            int numberOfHashKeysPerTable = hashingConfig.NumberOfMinHashesPerTable;
            int hashBuckets = hashingConfig.HashBuckets;
            byte[] subFingerprint = minHashService.Hash(fingerprint.Signature, numberOfHashTables * numberOfHashKeysPerTable);
            return new HashedFingerprint(
                GroupIntoHashTables(subFingerprint, numberOfHashTables, numberOfHashKeysPerTable, hashBuckets),
                fingerprint.SequenceNumber,
                fingerprint.StartsAt,
                clusters);
        }

        /// <summary>
        ///   Compute LSH hash buckets which will be inserted into hash tables.
        ///   Each fingerprint will have a candidate in each of the hash tables.
        /// </summary>
        /// <param name = "minHashes">Min Hashes gathered from every fingerprint [N = 100]</param>
        /// <param name = "numberOfHashTables">Number of hash tables [L = 25]</param>
        /// <param name = "numberOfHashesPerTable">Number of min hashes per key [N = 4]</param>
        /// <param name = "hashBucketsCount">Max number of hash buckets per hash table</param>
        /// <returns>Collection of Pairs with Key = Hash table index, Value = Hash bin</returns>
        protected virtual int[] GroupIntoHashTables(byte[] minHashes, int numberOfHashTables, int numberOfHashesPerTable, int hashBucketsCount)
        {
            int[] hashes = hashConverter.ToInts(minHashes, numberOfHashTables);

            if (hashBucketsCount == 0)
            {
                return hashes;
            }

            for (int i = 0; i < hashes.Length; ++i)
            {
                hashes[i] = System.Math.Abs(hashes[i] * LargePrime % hashBucketsCount);
            }

            return hashes;
        }
    }
}
