namespace SoundFingerprinting.Wavelets
{
    using System.Collections.Generic;

    /// <summary>
    ///   Wavelet decomposition algorithm
    /// </summary>
    internal interface IWaveletDecomposition
    {
        /// <summary>
        ///  Apply wavelet decomposition on entire set of fingerprint images
        /// </summary>
        /// <param name = "images">Frames to be decomposed</param>
        /// <param name="waveletNorm">Wavelet norm (comes from fingerprinting config)</param>
        void DecomposeImagesInPlace(IEnumerable<float[][]> images, double waveletNorm);

        /// <summary>
        ///   Apply wavelet decomposition on the selected image
        /// </summary>
        /// <param name = "image">Frame to be decomposed</param>
        /// <param name="waveletNorm">Wavelet norm, comes from fingerprinting config</param>
        void DecomposeImageInPlace(float[][] image, double waveletNorm);
    }
}