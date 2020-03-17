namespace SoundFingerprinting.LSH
{
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    internal interface ILocalitySensitiveHashingAlgorithm
    {
        HashedFingerprint Hash(Fingerprint fingerprint, HashingConfig hashingConfig);

        HashedFingerprint HashImage(Fingerprint fingerprint, HashingConfig hashingConfig);
    }
}