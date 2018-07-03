namespace SoundFingerprinting.Tests.Unit.Wavelets
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Wavelets;

    [TestFixture]
    public class StandardHaarWaveletDecompositionTest : AbstractTest
    {
        private IWaveletDecomposition waveletDecomposition;

        [SetUp]
        public void SetUp()
        {
            waveletDecomposition = new StandardHaarWaveletDecomposition();
        }

        [Test]
        public void StandardDecompositionTest()
        {
            const int Rows = 128;
            const int Cols = 32;
            float[][] frames = new float[Rows][];
            float[] concatenated = new float[Rows * Cols];
            for (int i = 0; i < Rows; i++)
            {
                frames[i] = TestUtilities.GenerateRandomSingleArray(Cols);
                Buffer.BlockCopy(frames[i], 0, concatenated, sizeof(float) * i * Cols, sizeof(float) * Cols);
            }

            AssertAreSame(Rows, Cols, frames, concatenated);
            waveletDecomposition.DecomposeImageInPlace(concatenated, Rows, Cols, 1d);
            DecomposeImageLocal(frames);
            AssertAreSame(Rows, Cols, frames, concatenated);
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
