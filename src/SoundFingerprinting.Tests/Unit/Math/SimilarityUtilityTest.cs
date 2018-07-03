namespace SoundFingerprinting.Tests.Unit.Math
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Math;

    [TestFixture]
    public class SimilarityUtilityTest : AbstractTest
    {
        private readonly ISimilarityUtility similarityUtility = new SimilarityUtility();
        private readonly IHashConverter hashConverter = new HashConverter();

        [Test]
        public void ShouldCorrectlyCalculateHammingDistanceBetweenLongs()
        {
            var length = 50000;

            for (int run = 0; run < 1000; run++)
            {
                var x = GenerateByteArray(length);
                var a = hashConverter.ToInts(x, length / 4);

                var y = GenerateByteArray(length);
                var b = hashConverter.ToInts(y, length / 4);

                var byteSimilarity = similarityUtility.CalculateHammingSimilarity(x, y);
                var longSimilarity = similarityUtility.CalculateHammingSimilarity(a, b, 4);

                Assert.AreEqual(byteSimilarity, longSimilarity);
            }
        }

        [Test]
        public void CalculateHammingDistanceCorrect()
        {
            byte[] first = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] second = { 1, 2, 3, 8, 5, 9, 7, 8, 11, 13 };

            var result = similarityUtility.CalculateHammingDistance(first, second);

            Assert.AreEqual(4, result);
        }

        [Test]
        public void CalculateHammingSimilarityCorrect()
        {
            byte[] first = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] second = { 1, 2, 3, 8, 5, 9, 7, 8, 11, 13 };

            var result = similarityUtility.CalculateHammingSimilarity(first, second);

            Assert.AreEqual(6, result);
        }

        [Test]
        public void CalculateJaccardSimilarityCorrect()
        {
            bool[] first = { true, true, false, true, false, true, false, false, true, true };
            bool[] second = { false, true, false, true, false, true, false, false, true, true };

            var result = similarityUtility.CalculateJaccardSimilarity(first, second);

            Assert.AreEqual(5f / 6, result, 0.0001);
        }

        private byte[] GenerateByteArray(int length)
        {
            var ran = new Random();
            byte[] random = new byte[length];
            ran.NextBytes(random);

            return random;
        }
    }
}
