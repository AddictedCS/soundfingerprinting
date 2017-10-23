namespace SoundFingerprinting.Wavelets
{
    /// <summary>
    ///   Wavelet decomposition algorithm
    /// </summary>
    internal interface IWaveletDecomposition
    {
        void DecomposeImageInPlace(float[] image, int rows, int cols, double waveletNorm);
    }
}
