namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    using System;

    public abstract class HaarWaveletDecomposition : IWaveletDecomposition
    {
        public abstract void DecomposeImageInPlace(float[][] image);

        protected void DecompositionStep(float[] array, int h)
        {
            float[] temp = new float[h];

            h /= 2;
            for (int i = 0; i < h; i++)
            {
                temp[i] = (float)((array[2 * i] + array[(2 * i) + 1]) / Math.Sqrt(2.0));
                temp[i + h] = (float)((array[2 * i] - array[(2 * i) + 1]) / Math.Sqrt(2.0));
            }

            for (int i = 0; i < (h * 2); i++)
            {
                array[i] = temp[i];
            }
        }
    }
}