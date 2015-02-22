namespace SoundFingerprinting.Wavelets
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class HaarWaveletDecomposition : WaveletDecomposition
    {
        public override void DecomposeImagesInPlace(IEnumerable<float[][]> logarithmizedSpectrum)
        {
            Parallel.ForEach(logarithmizedSpectrum, DecomposeImageInPlace);
        }

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