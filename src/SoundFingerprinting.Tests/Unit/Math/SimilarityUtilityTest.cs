namespace SoundFingerprinting.Tests.Unit.Math
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Math;

    [TestClass]
    public class SimilarityUtilityTest : AbstractTest
    {
        [TestMethod]
        public void CalculateHammingDistanceCorrect()
        {
            byte[] first = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] second = new byte[] { 1, 2, 3, 8, 5, 9, 7, 8, 11, 13 };

            var result = SimilarityUtility.CalculateHammingDistance(first, second);

            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void CalculateHammingSimilarityCorrect()
        {
            byte[] first = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] second = new byte[] { 1, 2, 3, 8, 5, 9, 7, 8, 11, 13 };

            var result = SimilarityUtility.CalculateHammingSimilarity(first, second);

            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void CalculateJaccardSimilarityCorrect()
        {
            bool[] first = new[] { true, true, false, true, false, true, false, false, true, true };
            bool[] second = new[] { false, true, false, true, false, true, false, false, true, true };

            var result = SimilarityUtility.CalculateJaccardSimilarity(first, second);

            Assert.AreEqual(5f / 6, result);
        }
    }
}
