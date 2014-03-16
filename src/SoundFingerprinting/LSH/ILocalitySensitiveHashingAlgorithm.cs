namespace SoundFingerprinting.LSH
{
    using SoundFingerprinting.Data;

    internal interface ILocalitySensitiveHashingAlgorithm
    {
        HashData Hash(bool[] fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable);
    }
}