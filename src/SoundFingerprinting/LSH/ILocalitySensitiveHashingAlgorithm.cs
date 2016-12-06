namespace SoundFingerprinting.LSH
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal interface ILocalitySensitiveHashingAlgorithm
    {
        HashedFingerprint Hash(Fingerprint fingerprint, int numberOfHashTables, int numberOfHashKeysPerTable, IEnumerable<string> clusters);
    }
}