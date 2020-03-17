namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class SamplesAggregatorTest
    {
        private const int SampleRate = 5512;
        
        private readonly ISamplesAggregator samplesAggregator = new SamplesAggregator();

        [Test]
        public void TestLessDataThanRequestedIsReceivedFromSamplesProvider()
        {
            const int secondsToRead = 55;

            // 20 20 10 seconds
            var queueBytesRead =
                new Queue<float[]>(
                    new[]
                        {
                            TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                            TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                            TestUtilities.GenerateRandomFloatArray(10 * SampleRate),
                            new float[0]
                        });
            Assert.Throws<AudioServiceException>(
                () =>
                samplesAggregator.ReadSamplesFromSource(
                    new QueueSamplesProvider(queueBytesRead), secondsToRead, SampleRate));
        }

        [Test]
        public void TestMoreDataIsReceivedThanRequested()
        {
            const int secondsToRead = 45;

            // 20 20 10 seconds
            var queueBytesRead = new Queue<float[]>(
                    new[]
                        {
                            TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                            TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                            TestUtilities.GenerateRandomFloatArray(10 * SampleRate),
                            new float[0]
                        });

            var samples = samplesAggregator.ReadSamplesFromSource(new QueueSamplesProvider(queueBytesRead), secondsToRead, SampleRate);

            Assert.AreEqual(secondsToRead * SampleRate, samples.Length);
        }

        [Test]
        public void TestExactAmountOfDataIsReceivedAsRequested()
        {
            const double secondsToRead = 65.8;

            // 20 20 20 seconds
            var floats = new[]
                {
                    TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                    TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                    TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                    TestUtilities.GenerateRandomFloatArray((int)(5.8 * SampleRate) /  4 * 4),
                    new float[0]
                };

            var queue = new Queue<float[]>(floats);

            var samples = samplesAggregator.ReadSamplesFromSource(new QueueSamplesProvider(queue), secondsToRead, SampleRate);

            Assert.AreEqual((int)(secondsToRead * SampleRate) / 4 * 4, samples.Length);
            int prevArrayLength = 0;
            for (int i = 0; i < floats.Length - 1; ++i)
            {
                float[] toCompare = new float[floats[i].Length];
                Array.Copy(samples, prevArrayLength, toCompare, 0, toCompare.Length);
                CollectionAssert.AreEqual(floats[i], toCompare);
                prevArrayLength += toCompare.Length;
            }
        }
    }
}
