namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class SamplesAggregatorTest : AbstractTest
    {
        private readonly ISamplesAggregator samplesAggregator = new SamplesAggregator();

        [Test]
        public void TestLessDataThanRequestedIsReceivedFromSamplesProvider()
        {
            const int SecondsToRead = 55;

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
                    new QueueSamplesProvider(queueBytesRead), SecondsToRead, SampleRate));
        }

        [Test]
        public void TestMoreDataIsReceivedThanRequested()
        {
            const int SecondsToRead = 45;

            // 20 20 10 seconds
            var queueBytesRead = new Queue<float[]>(
                    new[]
                        {
                            TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                            TestUtilities.GenerateRandomFloatArray(20 * SampleRate),
                            TestUtilities.GenerateRandomFloatArray(10 * SampleRate),
                            new float[0]
                        });

            var samples = samplesAggregator.ReadSamplesFromSource(new QueueSamplesProvider(queueBytesRead), SecondsToRead, SampleRate);

            Assert.AreEqual(SecondsToRead * SampleRate, samples.Length);
        }

        [Test]
        public void TestExactAmountOfDataIsReceivedAsRequested()
        {
            const double SecondsToRead = 65.8;

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

            var samples = samplesAggregator.ReadSamplesFromSource(new QueueSamplesProvider(queue), SecondsToRead, SampleRate);

            Assert.AreEqual((int)(SecondsToRead * SampleRate) / 4 * 4, samples.Length);
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
