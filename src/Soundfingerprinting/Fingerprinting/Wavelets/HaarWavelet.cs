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
        private static void DecomposeArray(float[] array)
        {
            int h = array.Length;
            for (int i = 0; i < h; i++) /*doesn't introduce any change in the final fingerprint image*/
                array[i] /= (float) Math.Sqrt(h); /*because works as a linear normalize*/
            float[] temp = new float[h];

            while (h > 1)
            {
                h /= 2;
                for (int i = 0; i < h; i++)
                {
                    temp[i] = (float) ((array[2*i] + array[2*i + 1])/Math.Sqrt(2));
                    temp[h + i] = (float) ((array[2*i] - array[2*i + 1])/Math.Sqrt(2));
                }
                for (int i = 0; i < 2*h; i++)
                {
                    array[i] = temp[i];
                }
            }
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

            for (int row = 0; row < rows /*128*/; row++) /*Decomposition of each row*/
                DecomposeArray(image[row]);

            for (int col = 0; col < cols /*32*/; col++) /*Decomposition of each column*/
            {
                float[] column = new float[rows]; /*Length of each column is equal to number of rows*/
                for (int row = 0; row < rows; row++)
                    column[row] = image[row][col]; /*Copying Column vector*/
                DecomposeArray(column);
                for (int row = 0; row < rows; row++)
                    image[row][col] = column[row];
            }
        }
    }
}