namespace SoundFingerprinting.Tests.Unit.Wavelets
{
    using System;
    using System.Linq;

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
                    Assert.AreEqual(frames[i][j], framesLocal[i][j], 0.00001);
                }
            }

            waveletDecomposition.DecomposeImageInPlace(frames, Math.Sqrt(2));
            DecomposeImageLocal(framesLocal, Math.Sqrt(2));

            for (int i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Cols; j++)
                {
                    Assert.AreEqual(frames[i][j], framesLocal[i][j], 0.5);
                }
            }


            double sumFrames = frames.Sum(target => target.Sum(d => Math.Abs(d)));
            double sumFrameLocal = framesLocal.Sum(target => target.Sum(d => Math.Abs(d)));
            Assert.AreEqual(sumFrames, sumFrameLocal, 0.5);
        }

        private void DecomposeArrayLocal(float[] array, double waveletNorm)
        {
            int h = array.Length;
            float[] temp = new float[h];
            while (h > 1)
            {
                h /= 2;
                for (int i = 0; i < h; i++)
                {
                    temp[i] = (float)((array[2 * i] + array[(2 * i) + 1]) / waveletNorm);
                    temp[h + i] = (float)((array[2 * i] - array[(2 * i) + 1]) / waveletNorm); 
                }

                for (int i = 0; i < 2 * h; i++)
                {
                    array[i] = temp[i];
                }
            }
        }

        private void DecomposeImageLocal(float[][] array, double waveletNorm)
        {
            int rows = array.GetLength(0);
            int cols = array[0].Length;
            for (int i = 0; i < rows; i++)
            {
                DecomposeArrayLocal(array[i], waveletNorm);
            }

            for (int i = 0; i < cols; i++)
            {
                float[] temp = new float[rows]; // each column has the size of
                for (int j = 0; j < rows; j++)
                {
                    temp[j] = array[j][i];
                }

                DecomposeArrayLocal(temp, waveletNorm);
                for (int j = 0; j < rows; j++)
                {
                    array[j][i] = temp[j];
                }
            }
        }
    }
}