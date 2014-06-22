namespace SoundFingerprinting.NeuralHasher.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.NeuralHasher.Utils;
    using SoundFingerprinting.Tests;

    [TestClass]
    public class BinaryOutputHelperTest : AbstractTest
    {
        private readonly IBinaryOutputHelper binaryOutputHelper = new BinaryOutputHelper();

        [TestMethod]
        public void TestBinaryOutputAreGeneratedAsExpected()
        {
            const int BinaryOutputLength = 2;
            var binaryCodes = binaryOutputHelper.GetBinaryCodes(BinaryOutputLength);

            Assert.AreEqual(4, binaryCodes.Length);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(BinaryOutputLength, binaryCodes[i].Length);
            }

            AssertBytesAreEqual(new[] { 0, 0 }, binaryCodes[0]);
            AssertBytesAreEqual(new[] { 1, 0 }, binaryCodes[1]);
            AssertBytesAreEqual(new[] { 0, 1 }, binaryCodes[2]);
            AssertBytesAreEqual(new[] { 1, 1 }, binaryCodes[3]);
        }

        private static void AssertBytesAreEqual(int[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}