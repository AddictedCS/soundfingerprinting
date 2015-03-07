namespace SoundFingerprinting.LSH
{
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal interface ILocalitySensitiveHashingAlgorithm
    {
        HashData Hash(bool[] fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable);
    }
}