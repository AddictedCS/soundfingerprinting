namespace SoundFingerprinting.Utils
{
    internal interface IFingerprintDescriptor
    {
        /// <summary>
        ///  Sets all other wavelet values to 0 except whose which make part of Top Wavelet [top wavelet &gt; 0 ? 1 : -1]
        /// </summary>
        /// <param name="frames">
        ///  Frames with 32 logarithmically spaced frequency bins
        /// </param>
        /// <param name="topWavelets">
        ///  The top Wavelets.
        /// </param>
        /// <param name="indexes">
        ///  Cached indexes
        /// </param>
        /// <returns>
        /// Signature signature. Array of encoded Boolean elements (wavelet signature)
        /// </returns>
        /// <remarks>
        ///   Negative Numbers = 01
        ///   Positive Numbers = 10
        ///   Zeros            = 00
        /// </remarks>
        IEncodedFingerprintSchema ExtractTopWavelets(float[] frames, int topWavelets, ushort[] indexes);
    }
}