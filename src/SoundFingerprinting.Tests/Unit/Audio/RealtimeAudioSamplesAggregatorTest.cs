namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class RealtimeAudioSamplesAggregatorTest
    {
        [Test]
        public void ShouldNotAllowSmallerThanMinSizeEntriesAtTheInput()
        {
            var realtimeAggregator = new RealtimeAudioSamplesAggregator(new IncrementalStaticStride(256), 10240);

            Assert.Throws<ArgumentException>(() => realtimeAggregator.Aggregate(new AudioSamples(TestUtilities.GenerateRandomFloatArray(10240 - 1), "cnn", 5512)));
        }
        
        [Test]
        public void ShouldBufferCorrectly()
        {
            // below instruction repeat 128 times
            // thus, last entry has to have at least 2048 elements
            // window  -----
            //          -----
            //                                 ----- // last one
            // 128 * 64 (overlap) = 8192 + 2048
            // 
            // the length of 1 fingerprint is 10240 as this is the minimal length that will allow generating full 128 * 32 log-image
            var stride = new IncrementalStaticStride(256);
            
            var minSize = 10240;
            var realtimeAggregator = new RealtimeAudioSamplesAggregator(stride, minSize);

            float[] prev = new float[minSize];
            for (int i = 0; i < 100; ++i)
            {
                float[] next = TestUtilities.GenerateRandomFloatArray(minSize);
                var toProcess = realtimeAggregator.Aggregate(new AudioSamples(next, "cnn", 5512));
                if (i == 0)
                {
                    Assert.AreSame(toProcess.Samples, next);
                    prev = next;
                    continue;
                }
                
                VerifyEndingsAreAttached(prev, next, toProcess, minSize, stride.NextStride);
                prev = next;
            }
        }

        private static void VerifyEndingsAreAttached(float[] prev, float[] next, AudioSamples current, int minSize, int strideSize)
        {
            int prefixLength = minSize - strideSize;
            Assert.AreEqual(minSize + prefixLength, current.Samples.Length);
            
            for (int i = 0; i < prefixLength; i++)
            {
                Assert.AreEqual(prev[prev.Length - prefixLength + i], current.Samples[i]);
            }

            for (int i = 0; i < next.Length; ++i)
            {
                Assert.AreEqual(next[i], current.Samples[prefixLength + i]);
            }
        }
    }
}