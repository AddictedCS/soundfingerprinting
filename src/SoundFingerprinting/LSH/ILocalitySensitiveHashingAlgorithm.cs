namespace SoundFingerprinting.LSH
{
    using SoundFingerprinting.Data;

    internal interface ILocalitySensitiveHashingAlgorithm
    {
        HashedFingerprint Hash(Fingerprint fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable);
    }
}