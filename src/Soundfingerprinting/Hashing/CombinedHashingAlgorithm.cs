namespace Soundfingerprinting.Hashing
{
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

        public long[] Hash(bool[] fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable)
        {
            byte[] subFingerprint = this.minHashService.Hash(fingerprint);
            return lshService.Hash(subFingerprint, numberOfHashTables, numberOfHashKeysPerTable);
        }
    }
}
