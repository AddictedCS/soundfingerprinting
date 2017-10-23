namespace SoundFingerprinting.Utils
{
    internal interface IFingerprintEncoder
    {
        /// <summary>
        ///   Encode the integer representation of the fingerprint into a Boolean array
        /// </summary>
        /// <param name = "concatenated">Concatenated fingerprint (frames concatenated)</param>
        /// <param name = "indexes">Sorted indexes with the first one with the highest value in array</param>
        /// <param name = "topWavelets">Number of top wavelets to encode</param>
        /// <returns>Encoded fingerprint</returns>
        IEncodedFingerprintSchema EncodeFingerprint(float[] concatenated, ushort[] indexes, int topWavelets);
    }
}