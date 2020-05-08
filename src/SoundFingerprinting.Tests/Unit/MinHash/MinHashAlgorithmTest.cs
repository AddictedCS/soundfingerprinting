﻿namespace SoundFingerprinting.Tests.Unit.MinHash
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;

    [TestFixture]
    public class MinHashAlgorithmTest
    {
        [Test]
        public void ShouldNotIdentifyTooManyHashesOnLastPosition()
        {
            var random = new Random(1);
            var permutations = new MaxEntropyPermutations();
            var minHash = new MinHashService(permutations);
            var counts = new List<int>();
            int maxIndex = permutations.GetPermutations().First().Length;
            Assert.AreEqual(255, maxIndex);
            for (int i = 0; i < 50000; ++i)
            {
                var schema = TestUtilities.GenerateRandomFingerprint(random, 200, 128, 32);
                byte[] hashes = minHash.Hash(schema, 25 * 4);
                int count = hashes.Count(last => last == maxIndex);
                counts.Add(count);
            }

            Console.WriteLine($"Avg. Permutations {counts.Average():0.000}");
            Console.WriteLine($"Max. {counts.Max()}");
            Console.WriteLine($"Min. {counts.Min()}");
            Assert.IsTrue(counts.Average() <= 0.25, "On average we expect no more than 0.25 elements in the schema to contain hashes at last position");
        }

        [Test]
        public void ShouldNotIdentifyTooManyHashesOnLastPositionForAdaptivePermutations()
        {
            var random = new Random(1);
            var permutations = new AdaptivePermutations(100, 128, 32);
            var minHash = new ExtendedMinHashService(permutations);
            var counts = new List<int>();
            int maxIndex = permutations.IndexesPerPermutation;
            Assert.AreEqual(128 * 32 * 2, maxIndex);
            for (int i = 0; i < 50000; ++i)
            {
                var schema = TestUtilities.GenerateRandomFingerprint(random, 200, 128, 32);
                int[] hashes = minHash.Hash(schema, 25 * 4);
                int count = hashes.Count(last => last == maxIndex);
                counts.Add(count);
            }

            Console.WriteLine($"Avg. Permutations {counts.Average():0.000}");
            Console.WriteLine($"Max. {counts.Max()}");
            Console.WriteLine($"Min. {counts.Min()}");
            Assert.AreEqual(0, counts.Average(), 0.0001, "On average we expect no more than 0.0 elements in the schema to contain hashes at last position");
        }

        [Test]
        public void ShouldBeAbleToGenerateMultipleTimesDifferentSignatures()
        {
            double howSimilarAreVectors = 0.4;
            int topWavelets = 200, vectorLength = 8192;
            var similarityUtility = new SimilarityUtility();

            double similarity = 0;
            int simulationRuns = 20000, agreeOn = 0;
            var random = new Random(1);
            for (int i = 0; i < simulationRuns; ++i)
            {
                var arrays = TestUtilities.GenerateSimilarFingerprints(random, howSimilarAreVectors, topWavelets, vectorLength);
                Assert.AreEqual(topWavelets, arrays.Item1.TrueCounts());
                Assert.AreEqual(topWavelets, arrays.Item2.TrueCounts());
                agreeOn += arrays.Item1.AgreeOn(arrays.Item2);
                similarity += similarityUtility.CalculateJaccardSimilarity(arrays.Item1.ToBools(), arrays.Item2.ToBools());
            }

            double averageSimilarityOnTrueBits = (double)agreeOn / simulationRuns;
            Assert.AreEqual(averageSimilarityOnTrueBits, howSimilarAreVectors * topWavelets, 1.0, $"Actual Average Similarity on True bits: {averageSimilarityOnTrueBits}");

            // values that match are counted one time, values that don't count twice (1 0 | 1 0) - don't match on 2 bits, even though they are generated from 1 wavelet
            double jaccardSimilarity = (howSimilarAreVectors * topWavelets) / ((2 * topWavelets) - (howSimilarAreVectors * topWavelets));
            Assert.AreEqual(jaccardSimilarity, similarity / simulationRuns, 0.1, "Jaccard Similarity is not as requested: " + (similarity / simulationRuns));
        }

        [Test]
        public void ShouldMatchAccordingToTheTheory()
        {
            var lsh = LocalitySensitiveHashingAlgorithm.Instance;
            int bands = 25; // segments
            int rows = 4;
            int topWavelets = 200;
            int vectorLength = 128 * 32 * 2;

            var hashingConfig = new DefaultHashingConfig();

            double[] howSimilars = { 0.3, 0.5, 0.6, 0.7, 0.75, 0.8, 0.85, 0.9 };
            double[] avgCandidatesFound = new double[howSimilars.Length];
            double[] probabilityOfAMatch = new double[howSimilars.Length];
            double[] atLeastOneCandidateFounds = new double[howSimilars.Length];

            Parallel.For(0, howSimilars.Length, i =>
            {
                var random = new Random(i);
                double howSimilar = howSimilars[i];
                double jaccardSimilarity = howSimilar * topWavelets / (2 * topWavelets - howSimilar * topWavelets);
                probabilityOfAMatch[i] = Math.Round(1 - Math.Pow(1 - Math.Pow(jaccardSimilarity, rows), bands), 4);

                int simulationRuns = 10000;
                int agreeOn = 0;
                int atLeastOneCandidateFound = 0;
                for (int j = 0; j < simulationRuns; ++j)
                {
                    var arrays = TestUtilities.GenerateSimilarFingerprints(random, howSimilar, topWavelets, vectorLength);
                    var hashed1 = lsh.Hash(new Fingerprint(arrays.Item1, 0, 0, Array.Empty<byte>()), hashingConfig);
                    var hashed2 = lsh.Hash(new Fingerprint(arrays.Item2, 0, 0, Array.Empty<byte>()), hashingConfig);
                    int agreeCount = AgreeOn(hashed1.HashBins, hashed2.HashBins);
                    if (agreeCount > 0)
                    {
                        atLeastOneCandidateFound++;
                    }

                    agreeOn += agreeCount;
                }

                avgCandidatesFound[i] = Math.Round((double)agreeOn / simulationRuns, 4);
                atLeastOneCandidateFounds[i] = Math.Round((double)atLeastOneCandidateFound / simulationRuns, 4);
            });

            Console.WriteLine("Bands {0}, Rows {1}, Top Wavelets {2}", bands, rows, topWavelets);

            string header = $"{"Actual Similarity",5}{"Th. At Least One",19}{"Pr. At Least One",18}{"Avg. Candidates Found",25}";

            Console.WriteLine(header);

            for (int i = 0; i < howSimilars.Length; ++i)
            {
                Console.WriteLine("{0,5:0.0000}{1,20:0.0000}{2,18:0.0000}{3,20:0.0000}", howSimilars[i], probabilityOfAMatch[i], atLeastOneCandidateFounds[i], avgCandidatesFound[i]);
            }

            for (int i = 0; i < howSimilars.Length; ++i)
            {
                Assert.AreEqual(probabilityOfAMatch[i], atLeastOneCandidateFounds[i], 0.05);
            }
        }

        private int AgreeOn(int[] x, int[] y)
        {
            return x.Where((t, i) => t == y[i]).Count();
        }
    }
}
