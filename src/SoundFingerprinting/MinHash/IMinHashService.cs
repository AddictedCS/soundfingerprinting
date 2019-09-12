namespace SoundFingerprinting.MinHash
{
    using SoundFingerprinting.Utils;

    internal interface IMinHashService<out T>
    {
        /// <summary>
        ///  Hash input array using N hash functions
        /// </summary>
        /// <param name="fingerprint">Fingerprint signature to hash</param>
        /// <param name="n">Number of hash functions to use</param>
        /// <returns>Min-hashed fingerprint, of size N</returns>
        T[] Hash(IEncodedFingerprintSchema fingerprint, int n);
    }
}