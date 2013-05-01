namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Non-Standard Haar wavelet decomposition
    /// Algorithm impelmented from http://grail.cs.washington.edu/projects/wavelets/article/wavelet1.pdf
    /// According to Fast Multiresolution Image Query, standard Haar wavelet decomposition works better on image querying.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class NonStandardHaarWaveletDecomposition : HaarWaveletDecomposition
    {
        #region IWaveletDecomposition Members

        public override void DecomposeImageInPlace(float[][] image)
        {
           DecomposeImage(image);
        }

        #endregion

        private void DecomposeImage(float[][] image)
        {
            int rows = image.GetLength(0); /*128*/
            int cols = image[0].Length; /*32*/
            float[] column = new float[rows];

            int w = cols, h = rows;
            while (w > 1 || h > 1)
            {
                // The order of decomposition is reversed because the image is 128x32 but we consider it reversed 32x128
                // final image does not change even with the reversed processing
                if (h > 1)
                {
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < rows; j++)
                        {
                            column[j] = image[j][i];
                        }

                        DecompositionStep(column, h);

                        for (int j = 0; j < rows; j++)
                        {
                            image[j][i] = column[j];
                        }
                    }
                }

                if (w > 1)
                {
                    for (int i = 0; i < h; i++)
                    {
                        DecompositionStep(image[i], w);
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