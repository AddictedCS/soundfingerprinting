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
                frames[i] = TestUtilities.GenerateRandomSingleArray(cols);
                Buffer.BlockCopy(frames[i], 0, concatenated, sizeof(float) * i * cols, sizeof(float) * cols);
            }

            AssertAreSame(rows, cols, frames, concatenated);
            
            var waveletDecomposition = new StandardHaarWaveletDecomposition();
            waveletDecomposition.DecomposeImageInPlace(concatenated, rows, cols, 1d);
            DecomposeImageLocal(frames);
            AssertAreSame(rows, cols, frames, concatenated);
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
