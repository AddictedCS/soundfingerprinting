namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests.Wavelets
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Fingerprinting.Wavelets;

    [TestClass]
    public class HaarWaveletTest : BaseTest
    {
        private IWaveletDecomposition waveletDecomposition;

        [TestInitialize]
        public void SetUp()
        {
            waveletDecomposition = new StandardHaarWaveletDecomposition();
        }

        [TestMethod]
        public void ComputeHaarWaveletsTest()
        {
            const int Rows = 128;
            const int Cols = 32;
            float[][] frames = new float[Rows][];
            float[][] framesLocal = new float[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                frames[i] = TestUtilities.GenerateRandomDoubleArray(Cols);
                framesLocal[i] = new float[Cols];
                frames[i].CopyTo(framesLocal[i], 0);
            }

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    Assert.AreEqual(true, (frames[i][j] - framesLocal[i][j]) < 0.00001);
                }
            }

            waveletDecomposition.DecomposeImageInPlace(frames);

            decimal sumFrames = frames.Sum(target => target.Sum(d => (decimal)Math.Abs(d)));

            decimal sumFrameLocal = framesLocal.Sum(target => target.Sum(d => (decimal)Math.Abs(d)));

            Assert.AreEqual(true, Math.Abs(sumFrames - sumFrameLocal) > (decimal)0.1);
            DecomposeImageLocal(framesLocal);
            for (int i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Cols; j++)
                {
                    Assert.AreEqual(true, (frames[i][j] - framesLocal[i][j]) < 0.0001);
                }
            }
        }

        private void DecomposeArrayLocal(float[] array)
        {
            int h = array.Length;
            for (int i = 0; i < h; i++)
            {
                array[i] /= (float)Math.Sqrt(h);
            }

            float[] temp = new float[h];

            while (h > 1)
            {
                h /= 2;
                for (int i = 0; i < h; i++)
                {
                    temp[i] = (float)((array[2 * i] + array[(2 * i) + 1]) / Math.Sqrt(2));
                    temp[h + i] = (float)((array[2 * i] - array[(2 * i) + 1]) / Math.Sqrt(2));
                }

                for (int i = 0; i < 2 * h; i++)
                {
                    array[i] = temp[i];
                }
            }
        }

        private void DecomposeImageLocal(float[][] array)
        {
            int rows = array.GetLength(0);
            int cols = array[0].Length;
            for (int i = 0; i < rows; i++)
            {
                DecomposeArrayLocal(array[i]);
            }

            for (int i = 0; i < cols; i++)
            {
                float[] temp = new float[rows]; // each column has the size of
                for (int j = 0; j < rows; j++)
                {
                    temp[j] = array[j][i];
                }

                DecomposeArrayLocal(temp);
                for (int j = 0; j < rows; j++)
                {
                    array[j][i] = temp[j];
                }
            }
        }
    }
}