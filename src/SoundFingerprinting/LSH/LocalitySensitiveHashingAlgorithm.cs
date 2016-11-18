namespace SoundFingerprinting.LSH
{
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;

    internal class LocalitySensitiveHashingAlgorithm : ILocalitySensitiveHashingAlgorithm
    {
        private readonly IMinHashService minHashService;
        private readonly IHashConverter hashConverter;

        public LocalitySensitiveHashingAlgorithm()
            : this(DependencyResolver.Current.Get<IMinHashService>(), DependencyResolver.Current.Get<IHashConverter>())
        {
        }

        internal LocalitySensitiveHashingAlgorithm(IMinHashService minHashService, IHashConverter hashConverter)
        {
            this.minHashService = minHashService;
            this.hashConverter = hashConverter;
        }

        public HashedFingerprint Hash(Fingerprint fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable)
        {
            byte[] subFingerprint = minHashService.Hash(fingerprint.Signature);
            return new HashedFingerprint(
                subFingerprint,
                GroupIntoHashTables(subFingerprint, numberOfHashTables, numberOfHashKeysPerTable),
                fingerprint.SequenceNumber,
                fingerprint.StartsAt);
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
            return hashConverter.ToLongs(minHashes, numberOfHashTables);
        }
    }
}
