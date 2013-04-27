namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    using System;

    public class DiscreteHaarWaveletDecomposition : IWaveletDecomposition
    {
        #region IWaveletDecomposition Members

        public void DecomposeImageInPlace(float[][] frames)
        {
           DecomposeImage(frames);
        }

        #endregion

        private static void DecomposeArray(float[] array, int w)
        {
            float[] temp = new float[array.Length];

            w /= 2;
            for (int i = 0; i < w; i++)
            {
                temp[i] = (float)((array[2 * i] + array[(2 * i) + 1]) / Math.Sqrt(2.0));
                temp[i + w] = (float)((array[2 * i] - array[(2 * i) + 1]) / Math.Sqrt(2.0));
            }

            for (int i = 0; i < (w * 2); i++)
            {
                array[i] = temp[i];
            }
        }

        private static void DecomposeImage(float[][] image)
        {
            int rows = image.GetLength(0); /*128*/
            int cols = image[0].Length; /*32*/
            float[] column = new float[rows];

            int w = cols, h = rows;
            while (w > 1 || h > 1)
            {
                if (w > 1)
                {
                    for (int i = 0; i < h; i++)
                    {
                        DecomposeArray(image[i], w);
                    }
                }

                if (h > 1)
                {
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < rows; j++)
                        {
                            column[j] = image[j][i];
                        }

                        DecomposeArray(column, h);

                        for (int j = 0; j < rows; j++)
                        {
                            image[j][i] = column[j];
                        }
                    }
                }

                if (w > 1)
                {
                    w = w >> 1;
                }

                if (h > 1)
                {
                    h = h >> 1;
                }
            }
        }
    }
}