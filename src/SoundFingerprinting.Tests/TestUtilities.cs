namespace SoundFingerprinting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Audio;

    using SoundFingerprinting.Query;
    using SoundFingerprinting.Utils;

    internal static class TestUtilities
    {
        public static AudioSamples GenerateRandomAudioSamples(int length)
        {
            return new AudioSamples(GenerateRandomFloatArray(length), string.Empty, 5512);
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

        public static IEnumerable<MatchedWith> GetMatchedWith(float[] queryAt, float[] resultAt)
        {
            for (int i = 0; i < queryAt.Length; ++i)
            {
                yield return new MatchedWith(queryAt[i], resultAt[i], 100);
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
        public static TinyFingerprintSchema GenerateRandomFingerprint(Random random, int topWavelets, int width, int height)
        {
            int length = width * height * 2;
            var schema = new TinyFingerprintSchema(length);
            for (int i = 0; i < topWavelets; ++i)
            {
                int index = random.Next(1, width * height);

                if (index % 2 == 0)
                    schema.SetTrueAt(index * 2);     // negative wavelet
                else
                    schema.SetTrueAt(index * 2 - 1); // positive wavelet
            }

            return schema;
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