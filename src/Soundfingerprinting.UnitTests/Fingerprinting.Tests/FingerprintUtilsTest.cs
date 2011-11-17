// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.DbStorage.Utils;

namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests
{
    /// <summary>
    ///   Fingerprint utilities test
    /// </summary>
    [TestClass]
    public class FingerprintUtilsTest : BaseTest
    {
        /// <summary>
        ///   Convert to float test
        /// </summary>
        [TestMethod]
        public void ConvertToFloatArrayTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            byte[] byteArray = TestUtilities.GenerateRandomInputByteArray(SAMPLES_TO_READ);
            float[] floatArray = ArrayUtils.GetFloatArrayFromByte(byteArray);
            Assert.AreEqual(byteArray.Length, floatArray.Length);

            for (int i = 0; i < SAMPLES_TO_READ; i++)
            {
                if (floatArray[i] == 1)
                    Assert.AreEqual(1, byteArray[i]);
                else if (floatArray[i] == 0)
                    Assert.AreEqual(0, byteArray[i]);
                else if (floatArray[i] == -1)
                    Assert.AreEqual(255, byteArray[i]);
                else
                    Assert.Fail("Bad conversion in #" + name);
            }
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }


        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        public void ConvertToDoubleArrayTest0()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            byte[] signal = TestUtilities.GenerateRandomByteArray(SAMPLES_TO_READ);
            bool silence = false;
            float[] d = ArrayUtils.GetDoubleArrayFromSamples(signal, SAMPLES_TO_READ, ref silence);
            for (int i = 0; i < d.Length; i++)
            {
                Assert.AreEqual(d[i], unchecked((sbyte) signal[i]));
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        public void ConvertToDoubleArrayTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            byte[] signal = TestUtilities.GenerateRandomByteArray(SAMPLES_TO_READ*2);
            bool silence = false;
            float[] d = ArrayUtils.GetDoubleArrayFromSamples(signal, SAMPLES_TO_READ, ref silence);
            for (int i = 0; i < d.Length; i++)
            {
                Assert.AreEqual(d[i], BitConverter.ToInt16(signal, i*2));
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void ConvertToDoubleArrayTest2()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            byte[] signal = TestUtilities.GenerateRandomByteArray(SAMPLES_TO_READ*4);
            bool silence = false;
            float[] d = ArrayUtils.GetDoubleArrayFromSamples(signal, SAMPLES_TO_READ, ref silence);
            for (int i = 0; i < d.Length; i++)
            {
                Assert.AreEqual(d[i], BitConverter.ToInt32(signal, i*4));
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        public void ConvertToDoubleArrayTest3()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            byte[] signal = TestUtilities.GenerateRandomByteArray(SAMPLES_TO_READ*4);
            bool silence = false;
            float[] d = ArrayUtils.GetDoubleArrayFromSamples(signal, SAMPLES_TO_READ, ref silence);
            for (int i = 0; i < d.Length; i++)
            {
                Assert.AreEqual(d[i], BitConverter.ToSingle(signal, i*4));
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void ConvertToDoubleArrayTest4()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            byte[] signal = TestUtilities.GenerateRandomByteArray(SAMPLES_TO_READ*16);
            bool silence = false;
            float[] d = ArrayUtils.GetDoubleArrayFromSamples(signal, SAMPLES_TO_READ, ref silence);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        public void GetByteArrayFromFloatTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            float[] f = TestUtilities.GenerateRandomInputFloatArray(4096);
            byte[] b = ArrayUtils.GetByteArrayFromFloat(f);
            Assert.AreEqual(f.Length, b.Length);
            for (int i = 0; i < f.Length; i++)
            {
                if (f[i] == 0)
                    Assert.AreEqual(0, b[i]);
                else if (f[i] == -1)
                    Assert.AreEqual(255, b[i]);
                else if (f[i] == 1)
                    Assert.AreEqual(1, b[i]);
                else
                    Assert.Fail("Invalid array");
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void GetByteArrayFromFloatExceptionTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            ArrayUtils.GetByteArrayFromFloat(null);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void GetFloatArrayFromByteExceptionTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            ArrayUtils.GetFloatArrayFromByte(null);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        public void GetSByteArrayFromByteTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            int arrayLength = 4096;
            byte[] array = TestUtilities.GenerateRandomInputByteArray(arrayLength);
            sbyte[] sByteArray = ArrayUtils.GetSByteArrayFromByte(array);
            for (int i = 0; i < arrayLength; i++)
            {
                if (array[i] == 0)
                    Assert.AreEqual(0, sByteArray[i]);
                else if (array[i] == 255)
                    Assert.AreEqual(-1, sByteArray[i]);
                else if (array[i] == 1)
                    Assert.AreEqual(1, sByteArray[i]);
                else
                    Assert.Fail("Invalid array");
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Convert to float array test
        /// </summary>
        [TestMethod]
        public void GetFloatArrayFromSByteTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            int arrayLength = 4096;
            byte[] array = TestUtilities.GenerateRandomInputByteArray(arrayLength);
            sbyte[] sByteArray = ArrayUtils.GetSByteArrayFromByte(array);
            for (int i = 0; i < arrayLength; i++)
            {
                if (array[i] == 0)
                    Assert.AreEqual(0, sByteArray[i]);
                else if (array[i] == 255)
                    Assert.AreEqual(-1, sByteArray[i]);
                else if (array[i] == 1)
                    Assert.AreEqual(1, sByteArray[i]);
                else
                    Assert.Fail("Invalid array");
            }

            double[] floatArray = ArrayUtils.GetDoubleArrayFromSByte(sByteArray);
            for (int i = 0; i < arrayLength; i++)
            {
                if (sByteArray[i] == 0)
                    Assert.AreEqual(0, floatArray[i]);
                else if (sByteArray[i] == -1)
                    Assert.AreEqual(-1, floatArray[i]);
                else if (sByteArray[i] == 1)
                    Assert.AreEqual(1, floatArray[i]);
                else
                    Assert.Fail("Invalid array");
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}