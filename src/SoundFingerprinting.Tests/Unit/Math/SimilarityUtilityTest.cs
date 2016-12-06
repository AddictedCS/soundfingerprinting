﻿namespace SoundFingerprinting.Tests.Unit.Math
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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

            long[] hashes1 = GenericHashBuckets();
            hashes1[0] = 0;
            long[] hashes2 = GenericHashBuckets();
            long[] hashes3 = GenericHashBuckets();

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
                new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 0, 0, Enumerable.Empty<string>()),
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

        [Test]
        public void ShouldAccumulateHammingDistanceSum()
        {
            const int CandidatesCount = 5;
            var trackReference = new ModelReference<int>(0);
            var subFingerprints = GetSubFingerprintsForTrack(trackReference, CandidatesCount);
            var acumulator = new ConcurrentDictionary<IModelReference, ResultEntryAccumulator>(); 

            similarityUtility.AccumulateHammingSimilarity(subFingerprints, new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 1, 0d, Enumerable.Empty<string>()), acumulator);

            int expectedHammingSimilaritySum = (GenericSignature().Length * CandidatesCount) - CandidatesCount + 1;
            Assert.AreEqual(expectedHammingSimilaritySum, acumulator[trackReference].HammingSimilaritySum);
            Assert.AreEqual(CandidatesCount, acumulator[trackReference].BestMatch.SubFingerprint.SubFingerprintReference.Id);
        }

        [Test]
        public void ShouldAccumulateHammingDistanceSumForMultipleTracks()
        {
            const int CandidatesCount = 10000;

            var trackReference0 = new ModelReference<int>(0);
            var subFingerprints0 = GetSubFingerprintsForTrack(trackReference0, CandidatesCount);
            var trackReference1 = new ModelReference<int>(1);
            var subFingerprints1 = GetSubFingerprintsForTrack(trackReference1, CandidatesCount);
            var trackReference2 = new ModelReference<int>(2);
            var subFingerprints2 = GetSubFingerprintsForTrack(trackReference2, CandidatesCount);

            var allSubs = subFingerprints0.Concat(subFingerprints1).Concat(subFingerprints2);
            var acumulator = new ConcurrentDictionary<IModelReference, ResultEntryAccumulator>();

            Parallel.ForEach(
                allSubs,
                new ParallelOptions { MaxDegreeOfParallelism = 10 },
                sub =>
                similarityUtility.AccumulateHammingSimilarity(
                    new List<SubFingerprintData> { sub },
                    new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 1, 0d, Enumerable.Empty<string>()),
                    acumulator));

            int expectedHammingSimilarity = (GenericSignature().Length * CandidatesCount) - CandidatesCount + 1;
            Assert.AreEqual(expectedHammingSimilarity, acumulator[trackReference0].HammingSimilaritySum);
            Assert.AreEqual(expectedHammingSimilarity, acumulator[trackReference1].HammingSimilaritySum);
            Assert.AreEqual(expectedHammingSimilarity, acumulator[trackReference2].HammingSimilaritySum);
            Assert.AreEqual(CandidatesCount, acumulator[trackReference0].BestMatch.SubFingerprint.SubFingerprintReference.Id);
            Assert.AreEqual(CandidatesCount, acumulator[trackReference1].BestMatch.SubFingerprint.SubFingerprintReference.Id);
            Assert.AreEqual(CandidatesCount, acumulator[trackReference2].BestMatch.SubFingerprint.SubFingerprintReference.Id);
        }

        [Test]
        public void ShouldSortMatchesProperly()
        {
            List<SubFingerprintData> subFingerprints = new List<SubFingerprintData>();
            var trackReference = new ModelReference<int>(0);
            const double OneFingerprintLength = 1.0d;
            const int CandidatesCount = 20;
            for (int i = 0; i < CandidatesCount; ++i)
            {
                var sub = new SubFingerprintData(GenericHashBuckets(), i, OneFingerprintLength * (CandidatesCount - i), new ModelReference<int>(i), trackReference);
                subFingerprints.Add(sub);
            } 

            var acumulator = new ConcurrentDictionary<IModelReference, ResultEntryAccumulator>(); 

            similarityUtility.AccumulateHammingSimilarity(subFingerprints, new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 1, 0d, Enumerable.Empty<string>()), acumulator);

            var expected = Enumerable.Range(1, 20);
            var actual = acumulator[trackReference].Matches.Select(m => m.SubFingerprint.SequenceAt).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }


        private IEnumerable<SubFingerprintData> GetSubFingerprintsForTrack(ModelReference<int> trackReference, int candidatesCount)
        {
            List<SubFingerprintData> subFingerprints = new List<SubFingerprintData>();
            const double OneFingerprintLength = 0.256;
            for (int i = 0; i < candidatesCount - 1; ++i)
            {
                var sub = new SubFingerprintData(GenericHashBuckets(), i, OneFingerprintLength * i, new ModelReference<int>(i), trackReference);
                sub.Hashes[0] = 0;
                subFingerprints.Add(sub);
            }

            subFingerprints.Add(new SubFingerprintData(GenericHashBuckets(), candidatesCount - 1, OneFingerprintLength * candidatesCount, new ModelReference<int>(candidatesCount), trackReference));
            return subFingerprints;
        }
    }
}
