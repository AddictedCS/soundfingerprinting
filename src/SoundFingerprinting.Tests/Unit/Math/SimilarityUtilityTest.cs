namespace SoundFingerprinting.Tests.Unit.Math
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class SimilarityUtilityTest : AbstractTest
    {
        private readonly ISimilarityUtility similarityUtility = new SimilarityUtility();

        [Test]
        public void ShouldSumUpHammingDistanceAccrossTracks()
        {
            var hammingSimilarities = new ConcurrentDictionary<IModelReference, ResultEntryAccumulator>();
            
            long[] hashes1 = new long[GenericHashBuckets.Length];
            Array.Copy(GenericHashBuckets, hashes1, hashes1.Length);
            hashes1[0] = 0;
            long[] hashes2 = new long[GenericHashBuckets.Length];
            Array.Copy(GenericHashBuckets, hashes2, hashes2.Length);
            long[] hashes3 = new long[GenericHashBuckets.Length];
            Array.Copy(GenericHashBuckets, hashes3, hashes3.Length);

            similarityUtility.AccumulateHammingSimilarity(
                new List<SubFingerprintData>
                    {
                        new SubFingerprintData(
                            hashes1,
                            1,
                            0d,
                            new ModelReference<int>(1),
                            new ModelReference<int>(1)),
                        new SubFingerprintData(
                            hashes2,
                            2,
                            1.48d,
                            new ModelReference<int>(1),
                            new ModelReference<int>(2)),
                        new SubFingerprintData(
                            hashes3,
                            3,
                            2.92d,
                            new ModelReference<int>(2),
                            new ModelReference<int>(2))
                    },
                new HashedFingerprint(GenericSignature, GenericHashBuckets, 0, 0),
                hammingSimilarities);

            Assert.AreEqual(2, hammingSimilarities.Count);
            Assert.AreEqual(49, hammingSimilarities[new ModelReference<int>(1)].HammingSimilaritySum);
            Assert.AreEqual(100, hammingSimilarities[new ModelReference<int>(2)].HammingSimilaritySum);
        }

        [Test]
        public void CalculateHammingDistanceCorrect()
        {
            byte[] first = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] second = new byte[] { 1, 2, 3, 8, 5, 9, 7, 8, 11, 13 };

            var result = similarityUtility.CalculateHammingDistance(first, second);

            Assert.AreEqual(4, result);
        }

        [Test]
        public void CalculateHammingSimilarityCorrect()
        {
            byte[] first = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] second = new byte[] { 1, 2, 3, 8, 5, 9, 7, 8, 11, 13 };

            var result = similarityUtility.CalculateHammingSimilarity(first, second);

            Assert.AreEqual(6, result);
        }

        [Test]
        public void CalculateJaccardSimilarityCorrect()
        {
            bool[] first = new[] { true, true, false, true, false, true, false, false, true, true };
            bool[] second = new[] { false, true, false, true, false, true, false, false, true, true };

            var result = similarityUtility.CalculateJaccardSimilarity(first, second);

            Assert.AreEqual(5f / 6, result, 0.0001);
        }
    }
}
