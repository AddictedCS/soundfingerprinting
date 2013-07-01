namespace SoundFingerprinting.Tests.Unit.Fingerprinting
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao.Utils;

    [TestClass]
    public class FingerprintUtilsTest : AbstractTest
    {
        private const double Epsilon = 0.0001;
        
        [TestMethod]
        public void ConvertToFloatArrayTest()
        {
            byte[] byteArray = TestUtilities.GenerateRandomInputByteArray(SamplesToRead);
            float[] floatArray = ArrayUtils.GetFloatArrayFromByte(byteArray);
            Assert.AreEqual(byteArray.Length, floatArray.Length);

            for (int i = 0; i < SamplesToRead; i++)
            {
                if (Math.Abs(floatArray[i] - 1) < Epsilon)
                {
                    Assert.AreEqual(1, byteArray[i]);
                }
                else if (Math.Abs(floatArray[i] - 0) < Epsilon)
                {
                    Assert.AreEqual(0, byteArray[i]);
                }
                else if (Math.Abs(floatArray[i] - -1) < Epsilon)
                {
                    Assert.AreEqual(255, byteArray[i]);
                }
                else
                {
                    Assert.Fail("Bad conversion");
                }
            }
        }

        [TestMethod]
        public void ConvertToDoubleArrayTest0()
        {
            byte[] signal = TestUtilities.GenerateRandomByteArray(SamplesToRead);
            bool silence = false;
            float[] d = ArrayUtils.GetDoubleArrayFromSamples(signal, SamplesToRead, ref silence);
            for (int i = 0; i < d.Length; i++)
            {
                Assert.AreEqual(d[i], unchecked((sbyte)signal[i]));
            }
        }

        [TestMethod]
        public void ConvertToDoubleArrayTest1()
        {
            byte[] signal = TestUtilities.GenerateRandomByteArray(SamplesToRead * 2);
            bool silence = false;
            float[] d = ArrayUtils.GetDoubleArrayFromSamples(signal, SamplesToRead, ref silence);
            for (int i = 0; i < d.Length; i++)
            {
                Assert.AreEqual(d[i], BitConverter.ToInt16(signal, i * 2));
            }
        }

        [TestMethod]
        public void ConvertToDoubleArrayTest2()
        {
            byte[] signal = TestUtilities.GenerateRandomByteArray(SamplesToRead * 4);
            bool silence = false;
            float[] d = ArrayUtils.GetDoubleArrayFromSamples(signal, SamplesToRead, ref silence);
            for (int i = 0; i < d.Length; i++)
            {
                Assert.AreEqual(d[i], BitConverter.ToInt32(signal, i * 4));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertToDoubleArrayTest4()
        {
            byte[] signal = TestUtilities.GenerateRandomByteArray(SamplesToRead * 16);
            bool silence = false;
            ArrayUtils.GetDoubleArrayFromSamples(signal, SamplesToRead, ref silence);
        }

        [TestMethod]
        public void GetByteArrayFromFloatTest()
        {
            float[] f = TestUtilities.GenerateRandomInputFloatArray(4096);
            byte[] b = ArrayUtils.GetByteArrayFromFloat(f);
            Assert.AreEqual(f.Length, b.Length);
            for (int i = 0; i < f.Length; i++)
            {
                if (Math.Abs(f[i] - 0) < Epsilon)
                {
                    Assert.AreEqual(0, b[i]);
                }
                else if (Math.Abs(f[i] - -1) < Epsilon)
                {
                    Assert.AreEqual(255, b[i]);
                }
                else if (Math.Abs(f[i] - 1) < Epsilon)
                {
                    Assert.AreEqual(1, b[i]);
                }
                else
                {
                    Assert.Fail("Invalid array");
                }
            }
        }

        public void GetSByteArrayFromByteTest()
        {
            const int ArrayLength = 4096;
            byte[] array = TestUtilities.GenerateRandomInputByteArray(ArrayLength);
            sbyte[] byteArray = ArrayUtils.GetSByteArrayFromByte(array);
            for (int i = 0; i < ArrayLength; i++)
            {
                if (array[i] == 0)
                {
                    Assert.AreEqual(0, byteArray[i]);
                }
                else if (array[i] == 255)
                {
                    Assert.AreEqual(-1, byteArray[i]);
                }
                else if (array[i] == 1)
                {
                    Assert.AreEqual(1, byteArray[i]);
                }
                else
                {
                    Assert.Fail("Invalid array");
                }
            }
        }

        [TestMethod]
        public void GetFloatArrayFromSByteTest()
        {
            const int ArrayLength = 4096;
            byte[] array = TestUtilities.GenerateRandomInputByteArray(ArrayLength);
            sbyte[] byteArray = ArrayUtils.GetSByteArrayFromByte(array);
            for (int i = 0; i < ArrayLength; i++)
            {
                if (array[i] == 0)
                {
                    Assert.AreEqual(0, byteArray[i]);
                }
                else if (array[i] == 255)
                {
                    Assert.AreEqual(-1, byteArray[i]);
                }
                else if (array[i] == 1)
                {
                    Assert.AreEqual(1, byteArray[i]);
                }
                else
                {
                    Assert.Fail("Invalid array");
                }
            }

            double[] floatArray = ArrayUtils.GetDoubleArrayFromSByte(byteArray);
            for (int i = 0; i < ArrayLength; i++)
            {
                if (byteArray[i] == 0)
                {
                    Assert.AreEqual(0, floatArray[i]);
                }
                else if (byteArray[i] == -1)
                {
                    Assert.AreEqual(-1, floatArray[i]);
                }
                else if (byteArray[i] == 1)
                {
                    Assert.AreEqual(1, floatArray[i]);
                }
                else
                {
                    Assert.Fail("Invalid array");
                }
            }
        }
    }
}