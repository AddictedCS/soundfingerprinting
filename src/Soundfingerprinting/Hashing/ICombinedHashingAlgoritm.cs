namespace Soundfingerprinting.Hashing
{
    public interface ICombinedHashingAlgoritm
    {
        long[] Hash(bool[] fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable);
    }
}