namespace SoundFingerprinting.NeuralHasher.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.NeuralHasher.Utils;
    using SoundFingerprinting.Tests;

    [TestClass]
    public class BinaryOutputUtilsTest : AbstractTest
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

            Assert.AreEqual(0, binaryCodes[0][0]);
            Assert.AreEqual(0, binaryCodes[0][1]);
            Assert.AreEqual(1, binaryCodes[1][0]);
            Assert.AreEqual(0, binaryCodes[1][1]);
            Assert.AreEqual(0, binaryCodes[2][0]);
            Assert.AreEqual(1, binaryCodes[2][1]);
            Assert.AreEqual(1, binaryCodes[3][0]);
            Assert.AreEqual(1, binaryCodes[3][1]);
        }
    }
}
