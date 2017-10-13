namespace SoundFingerprinting.Wavelets
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal abstract class HaarWaveletDecomposition : IWaveletDecomposition
    {
        public void DecomposeImagesInPlace(IEnumerable<float[][]> logarithmizedSpectrum, double waveletNorm)
        {
            Parallel.ForEach(logarithmizedSpectrum, spectrum => { DecomposeImageInPlace(spectrum, waveletNorm); });
        }

        public abstract void DecomposeImageInPlace(float[][] image, double waveletNorm);

        protected void DecompositionStep(float[] array, int h, double waveletNorm)
        {
            float[] temp = new float[h];

            h /= 2;
            for (int i = 0, j = 0; i < h; ++i, j = 2 * i)
            {
                temp[i] = (float)((array[j] + array[j + 1]) / waveletNorm);
                temp[i + h] = (float)((array[j] - array[j + 1]) / waveletNorm) ;
            }

            Buffer.BlockCopy(temp, 0, array, 0, sizeof(float) * (h * 2));
        }
    }
}