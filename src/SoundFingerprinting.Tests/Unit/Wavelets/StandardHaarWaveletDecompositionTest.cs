namespace SoundFingerprinting.Tests.Unit.Wavelets
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Wavelets;

    [TestFixture]
    public class StandardHaarWaveletDecompositionTest
    {
        [Test]
        public void StandardDecompositionTest()
        {
            const int rows = 128;
            const int cols = 32;
            float[][] frames = new float[rows][];
            float[] concatenated = new float[rows * cols];
            for (int i = 0; i < rows; i++)
            {
                frames[i] = TestUtilities.GenerateRandomSingleArray(cols, i);
                Buffer.BlockCopy(frames[i], 0, concatenated, sizeof(float) * i * cols, sizeof(float) * cols);
            }

            AssertAreSame(rows, cols, frames, concatenated);
            
            var waveletDecomposition = new StandardHaarWaveletDecomposition();
            waveletDecomposition.DecomposeImageInPlace(concatenated, rows, cols, 1d);
            DecomposeImageLocal(frames);
            AssertAreSame(rows, cols, frames, concatenated);
        }

        /// <summary>
        ///  Example from https://www.eecis.udel.edu/~amer/CISC651/wavelets_for_computer_graphics_Stollnitz.pdf
        ///  It may be worth exploring using Non-Standard Wavelet Transform where columns are processed first and then the rows
        ///  More details here: https://dsp.stackexchange.com/questions/58843/what-is-the-correct-order-of-operations-for-a-2d-haar-wavelet-decomposition 
        /// </summary>
        [Test]
        public void ShouldDecomposeAsExpected()
        {
            var wd = new StandardHaarWaveletDecomposition();

            var floats = new[] {8f, 4, 1, 3};

            wd.DecomposeImageInPlace(floats, 1, 4, 2d); // Let's use 2 as norm, to reconstruct the result more easily

            CollectionAssert.AreEqual(new[] {4, 2, 2, -1}, floats);
        }

        private void AssertAreSame(int rows, int cols, float[][] frames, float[] concatenated)
        {
            for (int i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    Assert.AreEqual(frames[i][j], concatenated[(i * cols) + j], 0.5);
                }
            }
        }

        private void DecomposeArray(float[] array)
        {
            int h = array.Length;
            float[] temp = new float[h];
            while (h > 1)
            {
                h /= 2;
                for (int i = 0; i < h; i++)
                {
                    temp[i] = array[2 * i] + array[(2 * i) + 1];
                    temp[h + i] = array[2 * i] - array[(2 * i) + 1];
                }

                Buffer.BlockCopy(temp, 0, array, 0, sizeof(float) * (h * 2));
            }
        }

        private void DecomposeImageLocal(float[][] array)
        {
            int rows = array.GetLength(0);
            int cols = array[0].Length;
            for (int i = 0; i < rows; i++)
            {
                DecomposeArray(array[i]);
            }

            for (int i = 0; i < cols; i++)
            {
                float[] temp = new float[rows];
                for (int j = 0; j < rows; j++)
                {
                    temp[j] = array[j][i];
                }

                DecomposeArray(temp);
                for (int j = 0; j < rows; j++)
                {
                    array[j][i] = temp[j];
                }
            }
        }
    }
}
