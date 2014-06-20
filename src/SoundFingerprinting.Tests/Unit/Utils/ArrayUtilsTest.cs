namespace SoundFingerprinting.Tests.Unit.Utils
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Utils;

    [TestClass]
    public class ArrayUtilsTest
    {
        [TestMethod]
        public void TestArrayIsConcatenatedCorrectly()
        {
            float[][] array = new[] { new float[] { 1, 2, 3 }, new float[] { 4, 5, 6 }, new float[] { 7, 8, 9 } };

            float[] concatenated = ArrayUtils.ConcatenateDoubleDimensionalArray(array);

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
    }
}
