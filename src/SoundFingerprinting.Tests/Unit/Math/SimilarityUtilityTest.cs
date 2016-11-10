namespace SoundFingerprinting.Tests.Unit.Math
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Math;

    [TestClass]
    public class SimilarityUtilityTest : AbstractTest
    {
        private readonly ISimilarityUtility similarityUtility = new SimilarityUtility();

        [TestMethod]
        public void ShouldSumUpHammingDistanceAccrossTracks()
        {
            var hammingSimilarities = new Dictionary<IModelReference, int>();
            similarityUtility.AccumulateHammingSimilarity(
                new List<SubFingerprintData>
                    {
                        new SubFingerprintData(
                            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                            1,
                            0d,
                            new ModelReference<int>(1),
                            new ModelReference<int>(1)),
                        new SubFingerprintData(
                            new byte[] { 1, 2, 3, 4, 5, 4, 7, 8, 11, 10 },
                            2,
                            1.48d,
                            new ModelReference<int>(1),
                            new ModelReference<int>(2)),
                        new SubFingerprintData(
                            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 10 },
                            3,
                            2.92d,
                            new ModelReference<int>(2),
                            new ModelReference<int>(2))
                    },
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                hammingSimilarities);

            Assert.AreEqual(2, hammingSimilarities.Count);
            Assert.AreEqual(10, hammingSimilarities[new ModelReference<int>(1)]);
            Assert.AreEqual(17, hammingSimilarities[new ModelReference<int>(2)]);
        }

        [TestMethod]
        public void CalculateHammingDistanceCorrect()
        {
            byte[] first = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] second = new byte[] { 1, 2, 3, 8, 5, 9, 7, 8, 11, 13 };

            var result = similarityUtility.CalculateHammingDistance(first, second);

            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void CalculateHammingSimilarityCorrect()
        {
            byte[] first = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] second = new byte[] { 1, 2, 3, 8, 5, 9, 7, 8, 11, 13 };

            var result = similarityUtility.CalculateHammingSimilarity(first, second);

            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void CalculateJaccardSimilarityCorrect()
        {
            bool[] first = new[] { true, true, false, true, false, true, false, false, true, true };
            bool[] second = new[] { false, true, false, true, false, true, false, false, true, true };

            var result = similarityUtility.CalculateJaccardSimilarity(first, second);

            Assert.AreEqual(5f / 6, result);
        }
    }
}
