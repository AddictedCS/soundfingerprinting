namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class RealtimeAudioSamplesAggregatorTest
    {
        [Test]
        public void ShouldIgnoreEmptyDataArraysIfTheyArrive()
        {
            var realtimeAggregator = new RealtimeAudioSamplesAggregator(10_176, new IncrementalStaticStride(551 * 2));
            for(int i = 0; i < 10; ++i)
            {
                var result = realtimeAggregator.Aggregate(new AudioSamples(Array.Empty<float>(), string.Empty, 5512));
                Assert.IsNull(result);
            }
        }
        
        /**
         * Size of one buffer is 5512 * 100 / 1_000 = 551
         * First 10_176 / 551 = 18, iterations no audio samples will be purged, then every second iteration an element will be purged
         * All in all: (100 - 18) / 2 = 41 elements will be purged.
         */
        [Test]
        public void ShouldAggregateSmallPiecesCorrectly()
        {
            var config = new DefaultFingerprintConfiguration();
            int minFingerprintSize = config.SpectrogramConfig.MinimumSamplesPerFingerprint, sampleRate = 5512, bufferSizeMilliseconds = 100, totalLengthMilliseconds = 10 * 1000, nonNull = 0;
            int incrementBy = 551;
            var realtimeAggregator = new RealtimeAudioSamplesAggregator(minFingerprintSize, new IncrementalStaticStride(incrementBy));

            for (int i = 0; i < totalLengthMilliseconds / bufferSizeMilliseconds; ++i)
            {
                var buffer = TestUtilities.GenerateRandomFloatArray(bufferSizeMilliseconds * sampleRate / 1000);
                var relativeTo = DateTime.UnixEpoch.AddMilliseconds(bufferSizeMilliseconds * i);
                var aggregated = realtimeAggregator.Aggregate(new AudioSamples(buffer, string.Empty, sampleRate, relativeTo));
                if (aggregated == null)
                {
                    // first 18 and every second input buffer will not return results to the caller
                    Assert.IsTrue(i < 19 || i % 2 == 1, $"{i}");
                }
                else
                {
                    nonNull++;
                    if (nonNull == 1)
                    {
                        int overshot = 19 * 551; // buffer is 551 samples long
                        Assert.AreEqual(0, Math.Abs(DateTime.UnixEpoch.Subtract(aggregated.RelativeTo).TotalMilliseconds), delta: 1);
                        Assert.AreEqual(overshot, aggregated.Samples.Length);
                    }
                }
            }
            
            Assert.AreEqual(Math.Round((10f * sampleRate - minFingerprintSize) / (551)), nonNull);
        }

        [Test]
        public void ShouldBufferFirstEntryCorrectly()
        {
            var config = new DefaultFingerprintConfiguration();
            int minSize = config.SpectrogramConfig.MinimumSamplesPerFingerprint;
            const int incrementBy = 1024;
            
            var realtimeAggregator = new RealtimeAudioSamplesAggregator(minSize, new IncrementalStaticStride(incrementBy));

            var a = realtimeAggregator.Aggregate(new AudioSamples(TestUtilities.GenerateRandomFloatArray(minSize), "cnn", 5512));
            
            Assert.AreEqual(minSize, a!.Samples.Length);

            var b = realtimeAggregator.Aggregate(new AudioSamples(TestUtilities.GenerateRandomFloatArray(minSize), "cnn", 5512));
            
            Assert.AreEqual(minSize - incrementBy + minSize, b!.Samples.Length);
        }
        
        [Test]
        public void ShouldBufferCorrectly()
        {
            // below instruction repeat 128 times
            // thus, last entry has to have at least 2048 elements
            // window  -----
            //          -----
            //                                 ----- // last one
            // 128 * 64 (overlap) = 8192 + 2048 - 64
            // 
            // the length of 1 fingerprint is 10176 as this is the minimal length that will allow generating full 128 * 32 log-image
            // because the stride divides the minimal length of the fingerprint evenly, it will not cache any ignored windows
            var stride = new IncrementalStaticStride(10_176 / 8);
            var config = new DefaultFingerprintConfiguration();
            int minSize = config.SpectrogramConfig.MinimumSamplesPerFingerprint;
            var realtimeAggregator = new RealtimeAudioSamplesAggregator(minSize, stride);

            float[] prev = new float[minSize];
            for (int i = 0; i < 100; ++i)
            {
                float[] next = TestUtilities.GenerateRandomFloatArray(minSize);
                var aggregated = realtimeAggregator.Aggregate(new AudioSamples(next, "cnn", 5512));
                Assert.IsNotNull(aggregated);
                if (i == 0)
                {
                    CollectionAssert.AreEqual(next, aggregated.Samples);
                    prev = next;
                    continue;
                }
                
                VerifyEndingsAreAttached(prev, next, aggregated.Samples, minSize, stride.NextStride);
                prev = next;
            }
        }

        private static void VerifyEndingsAreAttached(float[] prev, float[] next, float[] aggregated, int minSize, int strideSize)
        {
            int prefixLength = minSize - strideSize;
            Assert.AreEqual(minSize + prefixLength, aggregated.Length);
            
            for (int i = 0; i < prefixLength; i++)
            {
                Assert.AreEqual(prev[prev.Length - prefixLength + i], aggregated[i]);
            }

            for (int i = 0; i < next.Length; ++i)
            {
                Assert.AreEqual(next[i], aggregated[prefixLength + i]);
            }
        }
    }
}