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
    internal class StandardHaarWaveletDecomposition : HaarWaveletDecomposition
    {
        /// <summary>
        ///   Apply Haar Wavelet decomposition on the image
        /// </summary>
        /// <param name = "image">Image to be decomposed</param>
        public override void DecomposeImageInPlace(float[][] image)
        {
           // DecomposeImage(image);
        }

        public override void DecomposeImageInPlace(float[] image, int rows, int cols)
        {
            DecomposeImage(image, rows, cols);
        }

        private void Decomposition(float[] array)
        {
            int h = array.Length;
            while (h > 1)
            {
                DecompositionStep(array, h);
                h /= 2;
            }
        }

        private void Decomposition(float[] array, int row, int rows, int cols, float[] temp)
        {
            int h = cols; // array.Length;
            while (h > 1)
            {
                DecompositionStep(array, h, row, rows, cols, temp);
                h /= 2;
            }
        }

        private void DecompositionStep(float[] array, int h, int row, int rows, int cols, float[] temp)
        {
            h /= 2;
            int prefix = row * cols; // row * (length of the row)
            for (int i = 0, j = 0; i < h; ++i, j = 2 * i)
            {
                temp[i] = array[prefix + j] + array[prefix + j + 1];
                temp[i + h] = array[prefix + j] - array[prefix + j + 1];
            }

            Buffer.BlockCopy(temp, 0, array, prefix * sizeof(float), sizeof(float) * (h * 2));
        }

        private void DecomposeImage(float[] image, int rows, int cols)
        {
            // The order of decomposition is reversed because the image is 128x32 but we consider it reversed 32x128
            for (int col = 0; col < cols /*32*/; col++)
            {
                float[] column = new float[rows]; /*Length of each column is equal to number of rows*/
                for (int colIndex = 0; colIndex < rows; colIndex++)
                {
                    column[colIndex] = image[col + (colIndex * cols)]; /*Copying Column vector*/
                }

                Decomposition(column); /*Decomposition of each row*/
                for (int colIndex = 0; colIndex < rows; colIndex++)
                {
                    image[col  + (cols * colIndex)] = column[colIndex];
                }
            }

            float [] temp = new float[cols];
            for (int row = 0; row < rows /*128*/; row++)
            {
                Decomposition(image, row, rows, cols, temp); /*Decomposition of each row*/
            }
        }
    }
}