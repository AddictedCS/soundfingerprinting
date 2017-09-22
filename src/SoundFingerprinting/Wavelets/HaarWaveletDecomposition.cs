namespace SoundFingerprinting.Wavelets
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal abstract class HaarWaveletDecomposition : IWaveletDecomposition
    {
        public void DecomposeImagesInPlace(IEnumerable<float[][]> logarithmizedSpectrum)
        {
            Parallel.ForEach(logarithmizedSpectrum, DecomposeImageInPlace);
        }

        public abstract void DecomposeImageInPlace(float[][] image);

        protected void DecompositionStep(float[] array, int h)
        {
            float[] temp = new float[h];

            h /= 2;
            for (int i = 0, j = 0; i < h; ++i, j = 2 * i)
            {
                temp[i] = array[j] + array[j + 1];
                temp[i + h] = array[j] - array[j + 1];
            }

            Buffer.BlockCopy(temp, 0, array, 0, sizeof(float) * (h * 2));
        }
    }
}