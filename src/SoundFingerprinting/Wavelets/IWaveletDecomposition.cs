namespace SoundFingerprinting.Wavelets
{
    using System.Collections.Generic;

    using SoundFingerprinting.FFT;

    /// <summary>
    ///   Wavelet decomposition algorithm
    /// </summary>
    public interface IWaveletDecomposition
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

        void DecomposeImagesInPlace(IEnumerable<SpectralImage> images);

        void DecomposeImageInPlace(SpectralImage image);
    }

    public abstract class WaveletDecomposition : IWaveletDecomposition
    {
        public abstract void DecomposeImagesInPlace(IEnumerable<float[][]> images);

        public abstract void DecomposeImageInPlace(float[][] image);

        public void DecomposeImagesInPlace(IEnumerable<SpectralImage> images)
        {
            foreach (SpectralImage image in images)
            {
                DecomposeImageInPlace(image.Image);
            }
        }

        public void DecomposeImageInPlace(SpectralImage image)
        {
            DecomposeImageInPlace(image.Image);
        }
    }
}