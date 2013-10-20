namespace SoundFingerprinting.Tests.Unit.NeuralHashing
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.NeuralHasher.Utils;

    [TestClass]
    public class BinaryOutputUtilTest
    {
        [TestMethod]
        public void GenerateBinaryCodesTest()
        {
            const int GeneratedSize = 5;
            byte[][] list = BinaryOutputUtil.GetAllBinaryCodes(GeneratedSize);

            byte[] firstElement = new byte[GeneratedSize];
            byte[] lastElement = new byte[GeneratedSize];
            for (int i = 0; i < GeneratedSize; i++)
            {
                firstElement[i] = 0;
                lastElement[i] = 1;
            }

            Assert.AreEqual(Math.Pow(2, GeneratedSize), list.GetLength(0));

            byte[] first2Test = { 0, 0, 0, 0, 0 };
            byte[] lst2Test = { 1, 1, 1, 1, 1 };

            for (int i = 0; i < GeneratedSize; i++)
            {
                Assert.AreEqual(firstElement[i], first2Test[i]);
                Assert.AreEqual(lastElement[i], lst2Test[i]);
            }
        }

        [TestMethod]
        public void SaveGeneratedBinCodesTest()
        {
            BinaryOutputUtil.GetAllBinaryCodes(5);
            BinaryOutputUtil.SaveBinaryCodes("test");
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyFilenameSaveTest()
        {
            BinaryOutputUtil.GetAllBinaryCodes(5);
            BinaryOutputUtil.SaveBinaryCodes(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadUnknownBinCodesTest()
        {
            BinaryOutputUtil.LoadBinaryCodes("sdjhfkdshgljkahdkjlghd");
        }

        [TestMethod]
        public void L2NormTest()
        {
            float[] vector = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            const float Expected = 3.0f;
            float actual = BinaryOutputUtil.L2Norm(vector);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void VectorSubtractionTest()
        {
            float[] firstVec = { 0, 0, 0, 0, 0 };
            float[] secondVec = { -1, -1, -1, -1, -1 };
            float[] expected = { 1, 1, 1, 1, 1 };
            float[] actual = BinaryOutputUtil.VectorSubtraction(firstVec, secondVec);

            Assert.AreEqual(firstVec.Length, actual.Length);
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        public void ZeroVectorSubtractionTest()
        {
            float[] firstVec = { 0, 0, 0, 0, 0 };
            float[] secondVec = { 0, 0, 0, 0, 0 };
            float[] actual = BinaryOutputUtil.VectorSubtraction(firstVec, secondVec);

            Assert.AreEqual(firstVec.Length, actual.Length);
            foreach (float item in actual)
            {
                Assert.AreEqual(0, item);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SubtractDifferentVecLenthTest()
        {
            float[] firstVec = { 0, 0, 0, 0, 0, 0 };
            float[] secondVec = { -1, -1, -1, -1, -1 };
            BinaryOutputUtil.VectorSubtraction(firstVec, secondVec);
        }
    }
}