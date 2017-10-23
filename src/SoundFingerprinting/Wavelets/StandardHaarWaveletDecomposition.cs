namespace SoundFingerprinting.Wavelets
{
    using System;

    /// <summary>
    ///   Standart Haar wavelet decomposition algorithm.
    /// </summary>
    /// <remarks>
    /// Implemented according to the algorithm found here http://grail.cs.washington.edu/projects/wavelets/article/wavelet1.pdf
    /// According to Fast Multi-Resolution Image Query paper, Haar wavelet decomposition with standard basis function works better in image querying
    /// </remarks>
    internal class StandardHaarWaveletDecomposition : IWaveletDecomposition
    {
        public void DecomposeImageInPlace(float[] image, int rows, int cols, double waveletNorm)
        {
            DecomposeImage(image, rows, cols, waveletNorm);
        }

        private void DecompositionArray(float[] array, float[] temp, double waveletNorm)
        {
            int h = array.Length;
            while (h > 1)
            {
                DecompositionStep(array, h, 0, temp, waveletNorm);
                h /= 2;
            }
        }

        private void DecompositionRow(float[] array, int row, int cols, float[] temp, double waveletNorm)
        {
            int h = cols;
            while (h > 1)
            {
                DecompositionStep(array, h, row * cols, temp, waveletNorm);
                h /= 2;
            }
        }

        private void DecompositionStep(float[] array, int h, int prefix, float[] temp, double waveletNorm)
        {
            h /= 2;
            for (int i = 0, j = 0; i < h; ++i, j = 2 * i)
            {
                temp[i] = (float)((array[prefix + j] + array[prefix + j + 1]) / waveletNorm);
                temp[i + h] = (float)((array[prefix + j] - array[prefix + j + 1]) / waveletNorm);
            }

            Buffer.BlockCopy(temp, 0, array, prefix * sizeof(float), sizeof(float) * (h * 2));
        }

        private void DecomposeImage(float[] image, int rows, int cols, double waveletNorm)
        {
            float[] temp = new float[rows > cols ? rows : cols];
            float[] column = new float[rows]; /*Length of each column is equal to number of rows*/

            // The order of decomposition is reversed because the image is 128x32 but we consider it reversed 32x128
            for (int col = 0; col < cols /*32*/; col++)
            {
                for (int colIndex = 0; colIndex < rows; colIndex++)
                {
                    column[colIndex] = image[col + (colIndex * cols)]; /*Copying Column vector*/
                }

                DecompositionArray(column, temp, waveletNorm); /*Decomposition of each row*/
                for (int colIndex = 0; colIndex < rows; colIndex++)
                {
                    image[col  + (cols * colIndex)] = column[colIndex];
                }
            }

            for (int row = 0; row < rows /*128*/; row++)
            {
                DecompositionRow(image, row, cols, temp, waveletNorm); /*Decomposition of each row*/
            }
        }
    }
}
