// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.Fingerprinting;

namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests.Wavelets
{
    [TestClass]
    public class HaarWaveletTest : BaseTest
    {
        #region Utilities

        /// <summary>
        ///   Local implementation of the algorithm
        /// </summary>
        /// <param name = "array">Array to be decomposed</param>
        private static void DecomposeArray(float[] array)
        {
            int h = array.Length;
            for (int i = 0; i < h; i++)
                array[i] /= (float) (Math.Sqrt(h));
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
        ///   Local implementation of the algorithm
        /// </summary>
        /// <param name = "array">Array to be sorted</param>
        private static void DecomposeImage(float[][] array)
        {
            int rows = array.GetLength(0);
            int cols = array[0].Length;
            for (int i = 0; i < rows; i++)
                DecomposeArray(array[i]);
            for (int i = 0; i < cols; i++)
            {
                float[] temp = new float[rows]; // each column has the size of
                for (int j = 0; j < rows; j++)
                    temp[j] = array[j][i];

                DecomposeArray(temp);
                for (int j = 0; j < rows; j++)
                    array[j][i] = temp[j];
            }
        }

        #endregion

        /// <summary>
        ///   Compute Haar - Wavelets
        /// </summary>
        [TestMethod]
        public void ComputeHaarWaveletsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            const int rows = 128;
            const int cols = 32;
            float[][] frames = new float[rows][];
            float[][] framesLocal = new float[rows][];
            for (int i = 0; i < rows; i++)
            {
                frames[i] = TestUtilities.GenerateRandomDoubleArray(cols);
                framesLocal[i] = new float[cols];
                frames[i].CopyTo(framesLocal[i], 0);
            }
            /*Check if the array was copied*/
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    Assert.AreEqual(true, (frames[i][j] - framesLocal[i][j]) < 0.00001);

            FingerprintManager manager = new FingerprintManager();
            manager.WaveletDecomposition.DecomposeImageInPlace(frames);

            decimal sumFrames = frames.Sum(target => target.Sum(d => (decimal) Math.Abs(d)));

            decimal sumFrameLocal = framesLocal.Sum(target => target.Sum(d => (decimal) Math.Abs(d)));

            Assert.AreEqual(true, (Math.Abs(sumFrames - sumFrameLocal)) > (decimal) 0.1);
            DecomposeImage(framesLocal);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    Assert.AreEqual(true, (frames[i][j] - framesLocal[i][j]) < 0.0001);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}