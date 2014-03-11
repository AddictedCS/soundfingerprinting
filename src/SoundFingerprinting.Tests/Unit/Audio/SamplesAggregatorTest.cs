namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;

    [TestClass]
    public class SamplesAggregatorTest : AbstractTest
    {
        private const int DefaultSampleRate = 5512;

        private readonly ISamplesAggregator samplesAggregator;

        public SamplesAggregatorTest()
        {
            samplesAggregator = new SamplesAggregator();
        }

        [TestMethod]
        [ExpectedException(typeof(AudioServiceException))]
        public void TestLessDataThanRequestedIsReceivedFromSamplesProvider()
        {
            const int SecondsToRead = 55;

            // 20 20 10 seconds
            var queueBytesRead =
                new Queue<int>(
                    new[]
                        {
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4,
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4,
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4 / 2, 0
                        });

            samplesAggregator.ReadSamplesFromSource(
                new QueueSamplesProvider(queueBytesRead), SecondsToRead, DefaultSampleRate);
        }

        [TestMethod]
        public void TestMoreDataIsReceivedThanRequested()
        {
            const int SecondsToRead = 45;

            // 20 20 10 seconds
            var queueBytesRead =
                new Queue<int>(
                    new[]
                        {
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4,
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4,
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4 / 2
                        });

            var samples = samplesAggregator.ReadSamplesFromSource(
                new QueueSamplesProvider(queueBytesRead), SecondsToRead, DefaultSampleRate);

            Assert.AreEqual(SecondsToRead * DefaultSampleRate, samples.Length);
        }

        [TestMethod]
        public void TestExactAmountOfDataIsReceivedAsRequested()
        {
            const int SecondsToRead = 60;

            // 20 20 20 seconds
            var queueBytesRead =
                new Queue<int>(
                    new[]
                        {
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4,
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4,
                            DefaultSampleRate * SamplesAggregator.DefaultBufferLengthInSeconds * 4
                        });

            var samples = samplesAggregator.ReadSamplesFromSource(
                new QueueSamplesProvider(queueBytesRead), SecondsToRead, DefaultSampleRate);

            Assert.AreEqual(SecondsToRead * DefaultSampleRate, samples.Length);
        }
    }
}
