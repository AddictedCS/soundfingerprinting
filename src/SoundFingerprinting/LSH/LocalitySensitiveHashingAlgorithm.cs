namespace SoundFingerprinting.LSH
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;

    internal class LocalitySensitiveHashingAlgorithm : ILocalitySensitiveHashingAlgorithm
    {
        private const int LargePrime = 433494437;
        private const uint PrimeOffset = 2166136261;
        private const int FnvPrime = 16777619;
        
        private readonly IMinHashService<byte> minHashService;
        private readonly IHashConverter hashConverter = HashConverter.Instance;
        private readonly ConcurrentDictionary<int, IMinHashService<int>> extendedMinHashServices;

        internal LocalitySensitiveHashingAlgorithm(IMinHashService<byte> minHashService)
        {
            extendedMinHashServices = new ConcurrentDictionary<int, IMinHashService<int>>();
            this.minHashService = minHashService;
        }

        public static ILocalitySensitiveHashingAlgorithm Instance { get; } = new LocalitySensitiveHashingAlgorithm(MinHashService.MaxEntropy);

        public HashedFingerprint Hash(Fingerprint fingerprint, HashingConfig hashingConfig,  IEnumerable<string> clusters)
        {
            int numberOfHashTables = hashingConfig.NumberOfLSHTables;
            int numberOfHashKeysPerTable = hashingConfig.NumberOfMinHashesPerTable;
            int hashBuckets = hashingConfig.HashBuckets;
            byte[] subFingerprint = minHashService.Hash(fingerprint.Schema, numberOfHashTables * numberOfHashKeysPerTable);
            int[] hashBins = GroupIntoHashTables(subFingerprint, numberOfHashTables, numberOfHashKeysPerTable, hashBuckets);
            return new HashedFingerprint(hashBins, fingerprint.SequenceNumber, fingerprint.StartsAt, clusters);
        }

        public HashedFingerprint HashImage(Fingerprint fingerprint, HashingConfig hashingConfig, IEnumerable<string> clusters)
        {
            int n = hashingConfig.NumberOfLSHTables * hashingConfig.NumberOfMinHashesPerTable;
            int width = hashingConfig.Width;
            int height = hashingConfig.Height;
            var extendedMinHashService = extendedMinHashServices.GetOrAdd(width * height, key => new ExtendedMinHashService(new AdaptivePermutations(n, width, height)));
            int[] minHashes = extendedMinHashService.Hash(fingerprint.Schema, n);
            int[] hashed = HashMinHashes(minHashes, hashingConfig.NumberOfLSHTables, hashingConfig.NumberOfMinHashesPerTable);
            return new HashedFingerprint(hashed, fingerprint.SequenceNumber, fingerprint.StartsAt, clusters);
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
                hashes[i] = Math.Abs(hashes[i] * LargePrime % hashBucketsCount);
            }

            return hashes;
        }

        private int[] HashMinHashes(int[] signature, int l, int k)
        {
            int[] hash = new int[l];
            for (int table = 0; table < l; ++table)
            {
                long h = PrimeOffset;
                for (int i = 0; i < k; ++i)
                {
                    h *= FnvPrime;
                    h ^= signature[table * k + i];
                }

                hash[table] = (int) h;
            }

            return hash;
        }
    }
}
