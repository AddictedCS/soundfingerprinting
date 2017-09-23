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
        void DecomposeImagesInPlace(IEnumerable<float[][]> images);

        /// <summary>
        ///   Apply wavelet decomposition on the selected image
        /// </summary>
        /// <param name = "image">Frame to be decomposed</param>
        void DecomposeImageInPlace(float[][] image);

        void DecomposeImageInPlace(float[] image, int rows, int cols);
    }
}