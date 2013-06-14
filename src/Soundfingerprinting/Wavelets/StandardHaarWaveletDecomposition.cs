namespace SoundFingerprinting.Wavelets
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///   Standart Haar wavelet decomposition algorithm.
    ///   According to Fast Multi-Resolution Image Query paper, Haar wavelet decomposition with standard basis function works better in image querying
    /// </summary>
    /// <remarks>
    /// Implemented according to the algorithm found here http://grail.cs.washington.edu/projects/wavelets/article/wavelet1.pdf
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class StandardHaarWaveletDecomposition : HaarWaveletDecomposition
    {
        #region IWaveletDecomposition Members

        /// <summary>
        ///   Apply Haar Wavelet decomposition on the image
        /// </summary>
        /// <param name = "image">Image to be decomposed</param>
        public override void DecomposeImageInPlace(float[][] image)
        {
            DecomposeImage(image);
        }

        #endregion

        private void Decomposition(float[] array)
        {
            int h = array.Length;
            for (int i = 0; i < h; i++)
            {
                array[i] /= (float)Math.Sqrt(h);
            }

            while (h > 1)
            {
                DecompositionStep(array, h);
                h /= 2;
            }
        }

        /// <summary>
        ///   The standard 2-dimensional Haar wavelet decomposition involves one-dimensional decomposition of each row followed by a one-dimensional decomposition of each column of the result.
        /// </summary>
        /// <param name = "image">Image to be decomposed</param>
        private void DecomposeImage(float[][] image)
        {
            int rows = image.Length; /*128*/
            int cols = image[0].Length; /*32*/

            // The order of decomposition is reversed because the image is 128x32 but we consider it reversed 32x128
            for (int col = 0; col < cols /*32*/; col++)
            {
                float[] column = new float[rows]; /*Length of each column is equal to number of rows*/
                for (int row = 0; row < rows; row++)
                {
                    column[row] = image[row][col]; /*Copying Column vector*/
                }

                Decomposition(column); /*Decomposition of each row*/
                for (int row = 0; row < rows; row++)
                {
                    image[row][col] = column[row];
                }
            }

            for (int row = 0; row < rows /*128*/; row++)
            {
                Decomposition(image[row]); /*Decomposition of each row*/
            }
        }
    }
}