namespace Soundfingerprinting.Wavelets
{
    /// <summary>
    ///   Wavelet decomposition algorithm
    /// </summary>
    public interface IWaveletDecomposition
    {
        /// <summary>
        ///   Apply wavelet decomposition on the selected image
        /// </summary>
        /// <param name = "image">Frames to be decomposed</param>
        void DecomposeImageInPlace(float[][] image);
    }
}