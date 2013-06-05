namespace Soundfingerprinting.UnitTests.NeuralHashing.Tests
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Hashing.NeuralHashing.Utils;

    [TestClass]
    public class BinaryOutputUtilTest
    {
        [TestMethod]
        public void GenerateBinaryCodesTest()
        {
            const int generatedSize = 5;
            byte[][] list = BinaryOutputUtil.GetAllBinaryCodes(generatedSize);

            byte[] firstElement = new byte[generatedSize];
            byte[] lastElement = new byte[generatedSize];
            for (int i = 0; i < generatedSize; i++)
            {
                firstElement[i] = 0;
                lastElement[i] = 1;
            }

            Assert.AreEqual(Math.Pow(2, generatedSize), list.GetLength(0));

            byte[] first2Test = {0, 0, 0, 0, 0};
            byte[] lst2Test = {1, 1, 1, 1, 1};

            for (int i = 0; i < generatedSize; i++)
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
        [ExpectedException(typeof (ArgumentException))]
        public void EmptyFilenameSaveTest()
        {
            BinaryOutputUtil.GetAllBinaryCodes(5);
            BinaryOutputUtil.SaveBinaryCodes("");
        }

        [TestMethod]
        public void LoadBinCodesTest()
        {
            byte[][] expected = BinaryOutputUtil.GetAllBinaryCodes(5);
            byte[][] actual = BinaryOutputUtil.LoadBinaryCodes("test");

            Assert.AreEqual(expected.GetLength(0), actual.GetLength(0));

            for (int i = 0, n = actual.GetLength(0); i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Assert.AreEqual(expected[i][j], actual[i][j]);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof (FileNotFoundException))]
        public void LoadUnknownBinCodesTest()
        {
            BinaryOutputUtil.LoadBinaryCodes("sdjhfkdshgljkahdkjlghd");
        }

        [TestMethod]
        public void L2NormTest()
        {
            //Array to test: [1 1 1 1 1 1 1 1 1]
            float[] vector = {1, 1, 1, 1, 1, 1, 1, 1, 1};
            //L2Norm of vector is 3 ( 1*1 + 1*1 + ... + 1*1) ^ 0.5
            float expected = 3.0f;
            float actual = BinaryOutputUtil.L2Norm(vector);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void VectorSubtractionTest()
        {
            float[] firstVec = {0, 0, 0, 0, 0};
            float[] secondVec = {-1, -1, -1, -1, -1};
            float[] expected = {1, 1, 1, 1, 1};
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
            float[] firstVec = {0, 0, 0, 0, 0};
            float[] secondVec = {0, 0, 0, 0, 0};
            float[] actual = BinaryOutputUtil.VectorSubtraction(firstVec, secondVec);

            Assert.AreEqual(firstVec.Length, actual.Length);
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(0, actual[i]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void SubtractDifferentVecLenthTest()
        {
            float[] firstVec = {0, 0, 0, 0, 0, 0};
            float[] secondVec = {-1, -1, -1, -1, -1};
            BinaryOutputUtil.VectorSubtraction(firstVec, secondVec);
        }

        [TestMethod]
        public void FindMinL2NormTest()
        {
            // List<float[]> pool = BinaryOutputUtil.GetAllBinaryCodes(5);
            // float[] array = new float[5] { 1, 1, 1, 1, 1 };
            // float[] array2 = new float[5] { 1, 1, 1, 1, 0 };
            // List<float[]> list = new List<float[]>() { array, array2 };
            // KeyValuePair<int, int> result = BinaryOutputUtil.FindMinL2Norm(pool, list);
            // Assert.AreEqual(31, result.Key);
            // Assert.AreEqual(0, result.Value);
        }
    }
}