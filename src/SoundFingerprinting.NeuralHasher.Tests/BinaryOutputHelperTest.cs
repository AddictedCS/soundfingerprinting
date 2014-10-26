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

            AssertArraysAreEqual(new byte[] { 0, 0 }, binaryCodes[0]);

            AssertArraysAreEqual(new byte[] { 0, 0 }, binaryCodes[0]);
            AssertArraysAreEqual(new byte[] { 1, 0 }, binaryCodes[1]);
            AssertArraysAreEqual(new byte[] { 0, 1 }, binaryCodes[2]);
            AssertArraysAreEqual(new byte[] { 1, 1 }, binaryCodes[3]);
        }
    }
}