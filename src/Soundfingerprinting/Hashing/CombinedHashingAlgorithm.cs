namespace Soundfingerprinting.Hashing
{
    using System;

    using Soundfingerprinting.Hashing.LSH;
    using Soundfingerprinting.Hashing.MinHash;

    public class CombinedHashingAlgorithm : ICombinedHashingAlgoritm
    {
        private readonly IMinHashService minHashService;

        private readonly ILSHService lshService;

        public CombinedHashingAlgorithm(
            IMinHashService minHashService, ILSHService lshService)
        {
            this.minHashService = minHashService;
            this.lshService = lshService;
        }

        public Tuple<byte[], long[]> Hash(bool[] fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable)
        {
            byte[] subFingerprint = minHashService.Hash(fingerprint);
            return new Tuple<byte[], long[]>(subFingerprint, lshService.Hash(subFingerprint, numberOfHashTables, numberOfHashKeysPerTable));
        }
    }
}
