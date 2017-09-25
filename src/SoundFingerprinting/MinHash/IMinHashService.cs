namespace SoundFingerprinting.MinHash
{
    using SoundFingerprinting.Utils;

    internal interface IMinHashService
    {
        /// <summary>
        ///  Hash input array using N hash functions
        /// </summary>
        /// <param name="fingerprint">Fingerprint signature to hash</param>
        /// <param name="n">Number of hash functions to use</param>
        /// <returns>Minhashed fingerprint, of size N</returns>
        byte[] Hash(IEncodedFingerprintSchema fingerprint, int n);
    }
}