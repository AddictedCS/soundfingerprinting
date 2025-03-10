namespace SoundFingerprinting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Audio;
    using NUnit.Framework;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Utils;

    internal static class TestUtilities
    {
        public static AudioSamples GenerateRandomAudioSamples(int length, DateTime? relativeTo = null, int sampleRate = 5512)
        {
            return new AudioSamples(GenerateRandomFloatArray(length), string.Empty, sampleRate, relativeTo ?? DateTime.UtcNow);
        }

        public static Frames GenerateRandomFrames(int length, DateTime? relativeTo = null)
        {
            int framesPerSecond = 30;
            var frames = Enumerable.Range(0, length)
                .Select(index => new Frame(GenerateRandomFloatArray(128 * 72).Select(_ => _ / 32767).ToArray(), 128, 72, (float)index / framesPerSecond, (uint)index))
                .ToList();

            return new Frames(frames, string.Empty, framesPerSecond, relativeTo ?? DateTime.UtcNow);
        }

        public static float[] GenerateRandomFloatArray(int length, int seed = 0)
        {
            var random = new Random(seed == 0 ? (int)DateTime.Now.Ticks << 4 : seed);
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (float)random.NextDouble() * 32767;
            }

            return result;
        }

        public static float[] GenerateRandomSingleArray(int length, int seed = 0)
        {
            var random = new Random(seed == 0 ? (int)DateTime.Now.Ticks << 4 : seed);
            float[] d = new float[length];
            for (int i = 0; i < length; i++)
            {
                d[i] = (float)(32767 - (random.NextDouble() * 65535));
            }

            return d;
        }

        public static IEnumerable<MatchedWith> GetMatchedWith(int[] queryAt, int[] trackAt, int score = 100, float fingerprintLength = 1)
        {
            for (int sequenceNumber = 0; sequenceNumber < queryAt.Length; ++sequenceNumber)
            {
                yield return new MatchedWith((uint)queryAt[sequenceNumber], queryAt[sequenceNumber] * fingerprintLength, (uint)trackAt[sequenceNumber], trackAt[sequenceNumber] * fingerprintLength, score);
            }
        }

        public static Tuple<TinyFingerprintSchema, TinyFingerprintSchema> GenerateSimilarFingerprints(Random random, double similarityIndex, int topWavelets, int length)
        {
            var first = new TinyFingerprintSchema(length);
            var second = new TinyFingerprintSchema(length);
            var indexesTopWavelets = Enumerable.Range(0, length / 2)
                                               .OrderBy(x => random.NextDouble())
                                               .Take(topWavelets);

            foreach (int index in indexesTopWavelets)
            {
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
        
        public static Hashes GetRandomHashes(float length, MediaType mediaType = MediaType.Audio)
        {
            return GetRandomHashes((int)length, new Random(), false, 1, mediaType);
        }
        
        public static Hashes GetRandomHashes(int count, Random random, bool withOriginalPoints = false, float fingerprintLengthInSeconds = 1.48f, MediaType mediaType = MediaType.Audio)
        {
            var fingerprints = new List<HashedFingerprint>();
            for (int i = 0; i < count; ++i)
            {
                var hashes = new byte[sizeof(int) * 25];
                random.NextBytes(hashes);
                int[] intHashes = new int[25];
                Buffer.BlockCopy(hashes, 0, intHashes, 0, sizeof(int) * 25);
                byte[] originalPoint = Array.Empty<byte>();
                if (withOriginalPoints)
                {
                    originalPoint = new byte[100]; // small footprint for tests
                    random.NextBytes(originalPoint);
                }

                var hashData = new HashedFingerprint(intHashes, (uint) i, i * fingerprintLengthInSeconds, originalPoint);
                fingerprints.Add(hashData);
            }

            return new Hashes(fingerprints, fingerprints.Max(f => f.StartsAt + fingerprintLengthInSeconds), mediaType);
        }

        public static TinyFingerprintSchema GenerateRandomFingerprint(Random random, int topWavelets, int width, int height)
        {
            int length = width * height * 2;
            int[] trues = Enumerable.Range(0, topWavelets)
                .Select(entry => random.Next(0, length))
                .ToArray();
            
            return new TinyFingerprintSchema(length).SetTrueAt(trues);
        }
        
        public static void AssertHashesAreTheSame(Hashes expected, Hashes actual)
        {
            var tuples = expected.Join(actual, _ => _.SequenceNumber, _ => _.SequenceNumber, (a, b) => (a, b)).ToList();
            Assert.AreEqual(tuples.Count, expected.Count);
            foreach (var (first, second) in tuples)
            {
                Assert.AreEqual(first.StartsAt, second.StartsAt);
                Assert.AreEqual(first.SequenceNumber, second.SequenceNumber);
                CollectionAssert.AreEqual(first.HashBins, second.HashBins);
            }
        }
        
        public static Coverage GetCoverage(double coverageLength, double queryLength, double trackLength, IReadOnlyCollection<Gap> gaps)
        {
            const float sampleRate = 0.1f;
            var bestPath = new List<MatchedWith>();
            for (float i = 0; i < coverageLength; i += sampleRate)
            {
                uint sequenceNumber = (uint)(i / sampleRate);
                // query and track matches are symmetric
                var matched = new MatchedWith(sequenceNumber, queryMatchAt: i, sequenceNumber, trackMatchAt: i, score: 0d);
                if (!IsInsideGap(matched, gaps, fingerprintLength: 1.4862))
                {
                    bestPath.Add(matched);
                }
            }

            return new Coverage(bestPath, queryLength: queryLength, trackLength: trackLength, fingerprintLength: 1.4862f, permittedGap: 2d);
        }

        public static List<Gap> GetGaps(double[] gapsStartEnd)
        {
            var gaps = new List<Gap>();
            for (int k = 0; k < gapsStartEnd.Length; k += 2)
            {
                gaps.Add(new Gap(gapsStartEnd[k], gapsStartEnd[k + 1], isOnEdge: false));
            }

            return gaps;
        }
        
        public static AudioSamples Concatenate(AudioSamples first, AudioSamples second)
        {
            float[] concatenated = new float[first.Samples.Length + second.Samples.Length];
            Array.Copy(first.Samples, concatenated, first.Samples.Length);
            Array.Copy(second.Samples, 0, concatenated, first.Samples.Length, second.Samples.Length);
            return new AudioSamples(concatenated, string.Empty, first.SampleRate, first.RelativeTo);
        }
        
        private static bool IsInsideGap(MatchedWith matched, IEnumerable<Gap> gaps, double fingerprintLength)
        {
            return gaps.Any(gap => matched.TrackMatchAt + fingerprintLength >= gap.Start && matched.TrackMatchAt <= gap.End);
        }

        private static void Agree(float value, TinyFingerprintSchema first, int index, TinyFingerprintSchema second)
        {
            EncodeWavelet(value, first, index);
            EncodeWavelet(value, second, index);
        }

        private static void Disagree(float value, TinyFingerprintSchema first, int index, TinyFingerprintSchema second)
        {
            EncodeWavelet(value, first, index);
            EncodeWavelet(-1 * value, second, index);
        }

        private static void EncodeWavelet(float value, TinyFingerprintSchema array, int index)
        {
            if (value > 0)
            {
                array.SetTrueAt(index * 2);
            }
            else if (value < 0)
            {
                array.SetTrueAt(index * 2 + 1);
            }
        }
    }
}