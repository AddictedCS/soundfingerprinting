namespace Soundfingerprinting.Hashing
{
    using System;

    public interface ICombinedHashingAlgoritm
    {
        Tuple<byte[], long[]> Hash(bool[] fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable);
    }
}