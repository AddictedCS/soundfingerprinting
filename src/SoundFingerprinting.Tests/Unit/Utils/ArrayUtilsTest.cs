namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class ArrayUtilsTest
    {
        [Test]
        public void TestArrayIsConcatenatedCorrectly()
        {
            float[][] array = new[] { new float[] { 1, 2, 3 }, new float[] { 4, 5, 6 }, new float[] { 7, 8, 9 } };

            float[] concatenated = ConcatenateDoubleDimensionalArray(array);

            AssertArraysAreEqual(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, concatenated);
        }

        private void AssertArraysAreEqual(float[] expected, float[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        private float[] ConcatenateDoubleDimensionalArray(float[][] array)
        {
            int rows = array.GetLength(0);
            int cols = array[0].Length;
            float[] concatenated = new float[rows * cols];
            for (int row = 0; row < rows; row++)
            {
                Buffer.BlockCopy(array[row], 0, concatenated, row * array[row].Length * 4, array[row].Length * 4);
            }

            return concatenated;
        }
    }
}
