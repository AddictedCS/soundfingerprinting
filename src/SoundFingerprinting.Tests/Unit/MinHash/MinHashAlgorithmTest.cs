namespace SoundFingerprinting.Tests.Unit.MinHash
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
    using SoundFingerprinting.Utils;

    [TestFixture]
    public class MinHashAlgorithmTest
    {
        private readonly LocalitySensitiveHashingAlgorithm lsh = new LocalitySensitiveHashingAlgorithm(new MinHashService(new DefaultPermutations()), new HashConverter());

        [Test]
        public void ShouldBeAbleToGenerateMultipleTimesDifferentSignatures()
        {
            double howSimilarAreVectors = 0.4;
            int topWavelets = 200, vectorLength = 8192;
            var similarityUtility = new SimilarityUtility();

            double similarity = 0;
            int simulationRuns = 20000, aggreeOn = 0;
            for (int i = 0; i < simulationRuns; ++i)
            {
                var arrays = this.GenerateVectors(howSimilarAreVectors, topWavelets, vectorLength);
                Assert.AreEqual(topWavelets, arrays.Item1.TrueCounts());
                Assert.AreEqual(topWavelets, arrays.Item2.TrueCounts());
                aggreeOn += arrays.Item1.AgreeOn(arrays.Item2);
                similarity += similarityUtility.CalculateJaccardSimilarity(arrays.Item1.ToBools(), arrays.Item2.ToBools());
            }

            double averageSimilarityOnTrueBits = (double)aggreeOn / simulationRuns;
            Assert.AreEqual(
                averageSimilarityOnTrueBits,
                howSimilarAreVectors * topWavelets,
                1.0,
                "Actual Average Similarity on True bits: " + averageSimilarityOnTrueBits);

            // values that match are counted one time, values that don't count twice (1 0 | 1 0) - don't match on 2 bits, even though they are generated from 1 wavelet
            double jaccardSimilarity = (howSimilarAreVectors * topWavelets) / ((2 * topWavelets) - (howSimilarAreVectors * topWavelets));
            Assert.AreEqual(jaccardSimilarity, similarity / simulationRuns, 0.1, "Jaccard Similarity is not as requested: " + (similarity / simulationRuns));
        }

        [Test]
        public void ShouldMatchAccordingToTheTheory()
        {
            int bands = 25; // segments
            int rows = 4;
            int topWavelets = 200;
            int vectorLength = 8192;

            var hashingConfig = new DefaultHashingConfig();

            double[] howSimilars = { 0.3, 0.5, 0.6, 0.7, 0.75, 0.8, 0.85, 0.9 };
            double[] avgCandidatesFound = new double[howSimilars.Length];
            double[] probabilityOfAMatch = new double[howSimilars.Length];
            double[] atLeastOneCandidateFounds = new double[howSimilars.Length];

            Parallel.For(0, howSimilars.Length, i =>
            {
                double howSimilar = howSimilars[i];
                double jaccardSimilarity = howSimilar * topWavelets / (2 * topWavelets - howSimilar * topWavelets);
                probabilityOfAMatch[i] = Math.Round(1 - Math.Pow(1 - Math.Pow(jaccardSimilarity, rows), bands), 4);

                int simulationRuns = 50000;
                int agreeOn = 0;
                int atLeastOneCandidateFound = 0;
                for (int j = 0; j < simulationRuns; ++j)
                {
                    var arrays = GenerateVectors(howSimilar, topWavelets, vectorLength);
                    var hashed1 = lsh.Hash(new Fingerprint(arrays.Item1, 0, 0), hashingConfig, new List<string>());
                    var hashed2 = lsh.Hash(new Fingerprint(arrays.Item2, 0, 0), hashingConfig, new List<string>());
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
                Console.WriteLine(
                        "{0,5:0.0000}{1,20:0.0000}{2,18:0.0000}{3,20:0.0000}",
                        howSimilars[i],
                        probabilityOfAMatch[i],
                        atLeastOneCandidateFounds[i],
                        avgCandidatesFound[i]);
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

        private Tuple<TinyFingerprintSchema, TinyFingerprintSchema> GenerateVectors(double similarityIndex, int topWavelets, int length)
        {
            var random = new Random();

            var first = new TinyFingerprintSchema(length);
            var second = new TinyFingerprintSchema(length);
            var unique = new HashSet<int>();
            for (int i = 0; i < topWavelets; ++i)
            {
                int index = random.Next(length / 2);
                if (unique.Contains(2 * index) || unique.Contains(2 * index + 1))
                {
                    i--;
                    continue;
                }

                unique.Add(2 * index);
                unique.Add(2 * index + 1);

                float value = random.NextDouble() > 0.5 ? -1 : 1;
                if (random.NextDouble() > similarityIndex)
                {
                    Disagree(value, first, index, second);
                }
                else
                {
                    Agree(value, first, index, second);
                }
            }

            return Tuple.Create(first, second);
        }

        private void Agree(float value, TinyFingerprintSchema first, int index, TinyFingerprintSchema second)
        {
            EncodeWavelet(value, first, index);
            EncodeWavelet(value, second, index);
        }

        private void Disagree(float value, TinyFingerprintSchema first, int index, TinyFingerprintSchema second)
        {
            EncodeWavelet(value, first, index);
            EncodeWavelet(-1 * value, second, index);
        }

        private void EncodeWavelet(float value, TinyFingerprintSchema array, int index)
        {
            if (value > 0)
            {
                array.SetTrueAt(index * 2);
            }
            else if (value < 0)
            {
                array.SetTrueAt((index * 2) + 1);
            }
        }
    }
}
