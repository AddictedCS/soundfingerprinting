namespace SoundFingerprinting.Utils
{
    internal interface IFingerprintDescriptor
    {
        /// <summary>
        ///   Encode the integer representation of the fingerprint into Boolean array
        /// </summary>
        /// <param name = "concatenated">Concatenated fingerprint (frames concatenated)</param>
        /// <param name = "indexes">Sorted indexes with the first one with the highest value in array</param>
        /// <param name = "topWavelets">Number of top wavelets to encode</param>
        /// <returns>Encoded fingerprint</returns>
        IEncodedFingerprintSchema EncodeFingerprint(float[] concatenated, ushort[] indexes, int topWavelets);

        /// <summary>
        /// Sets all other wavelet values to 0 except whose which make part of Top Wavelet [top wavelet &gt; 0 ? 1 : -1]
        /// </summary>
        /// <param name="frames">
        /// Frames with 32 logarithmically spaced frequency bins
        /// </param>
        /// <param name="topWavelets">
        /// The top Wavelets.
        /// </param>
        /// <returns>
        /// Signature signature. Array of encoded Boolean elements (wavelet signature)
        /// </returns>
        /// <remarks>
        ///   Negative Numbers = 01
        ///   Positive Numbers = 10
        ///   Zeros            = 00
        /// </remarks>
        IEncodedFingerprintSchema ExtractTopWavelets(float[] frames, int topWavelets);
    }
}