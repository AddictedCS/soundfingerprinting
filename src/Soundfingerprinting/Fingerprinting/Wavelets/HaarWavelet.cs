// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    /// <summary>
    ///   Haar wavelet decomposition algorithm
    /// </summary>
    public class HaarWavelet : IWaveletDecomposition
    {
        #region IWaveletDecomposition Members

        /// <summary>
        ///   Apply Haar Wavelet decomposition on the frames
        /// </summary>
        /// <param name = "frames">Frames to be decomposed</param>
        public void DecomposeImageInPlace(float[][] frames)
        {
            DecomposeImage(frames);
        }

        #endregion

        /// <summary>
        ///   Decomposition taken from
        ///   Wavelets for Computer Graphics: A Primer Part by Eric J. Stollnitz Tony D. DeRose David H. Salesin
        /// </summary>
        /// <param name = "array">Array to be decomposed</param>
        private static void DecomposeArray(float[] vec, int n, int w)
        {
            int i = 0;
            float[] vecp = new float[n];
            for (i = 0; i < n; i++)
                vecp[i] = 0;

            w /= 2;
            for (i = 0; i < w; i++)
            {
                vecp[i] = (float)((vec[2 * i] + vec[2 * i + 1]) / Math.Sqrt(2.0));
                vecp[i + w] = (float)((vec[2 * i] - vec[2 * i + 1]) / Math.Sqrt(2.0));
            }

            for (i = 0; i < (w * 2); i++)
                vec[i] = vecp[i];
        }

        /// <summary>
        ///   The standard 2-dimensional Haar wavelet decomposition involves one-dimensional decomposition of each row
        ///   followed by a one-dimensional decomposition of each column of the result.
        /// </summary>
        /// <param name = "image">Image to be decomposed</param>
        private static void DecomposeImage(float[][] image)
        {
            int rows = image.GetLength(0); /*128*/
            int cols = image[0].Length; /*32*/
            float[] temp_row = new float[cols];
            float[] temp_col = new float[rows];

            int i = 0, j = 0;
            int w = cols, h = rows;
            while (w > 1 || h > 1)
            {
                if (w > 1)
                {
                    for (i = 0; i < h; i++)
                    {
                        for (j = 0; j < cols; j++)
                            temp_row[j] = image[i][j];

                        DecomposeArray(temp_row, cols, w);

                        for (j = 0; j < cols; j++)
                            image[i][j] = temp_row[j];
                    }
                }

                if (h > 1)
                {
                    for (i = 0; i < w; i++)
                    {
                        for (j = 0; j < rows; j++)
                            temp_col[j] = image[j][i];
                        DecomposeArray(temp_col, rows, h);
                        for (j = 0; j < rows; j++)
                            image[j][i] = temp_col[j];
                    }
                }

                if (w > 1)
                    w /= 2;
                if (h > 1)
                    h /= 2;
            }
        }
    }
}