namespace SoundFingerprinting.LSH
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    internal interface ILocalitySensitiveHashingAlgorithm
    {
        HashedFingerprint Hash(Fingerprint fingerprint, HashingConfig hashingConfig, IEnumerable<string> clusters);
    }
}