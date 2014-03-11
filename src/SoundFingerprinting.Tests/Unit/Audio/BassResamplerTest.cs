namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;

    using Un4seen.Bass;

    [TestClass]
    public class BassResamplerTest : AbstractTest
    {
        private BassResampler resampler;

        private Mock<IBassServiceProxy> proxy;

        private Mock<IBassStreamFactory> streamFactory;

        private Mock<ISamplesAggregator> samplesAggregator;

        [TestInitialize]
        public void SetUp()
        {
            proxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);
            streamFactory = new Mock<IBassStreamFactory>(MockBehavior.Strict);
            samplesAggregator = new Mock<ISamplesAggregator>(MockBehavior.Strict);

            resampler = new BassResampler(proxy.Object, streamFactory.Object, samplesAggregator.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            proxy.VerifyAll();
            streamFactory.VerifyAll();
            samplesAggregator.VerifyAll();
        }

        [TestMethod]
        public void TestResample()
        {
            const int SourceStream = 100;
            const int MixerStream = 101;
            const int Seconds = 50;
            const int StartAt = 0;
            float[] samplesToReturn = new float[1024];

            streamFactory.Setup(f => f.CreateMixerStream(SampleRate)).Returns(MixerStream);
            proxy.Setup(p => p.CombineMixerStreams(MixerStream, SourceStream, BASSFlag.BASS_MIXER_FILTER)).Returns(true);
            proxy.Setup(p => p.FreeStream(SourceStream)).Returns(true);
            proxy.Setup(p => p.FreeStream(MixerStream)).Returns(true);
            samplesAggregator.Setup(s => s.ReadSamplesFromSource(It.IsAny<ISamplesProvider>(), Seconds, SampleRate))
                .Returns(samplesToReturn);

            float[] samples = resampler.Resample(
                SourceStream, SampleRate, Seconds, StartAt, mixerStream => new QueueSamplesProvider(new Queue<int>()));

            Assert.AreEqual(samplesToReturn.Length, samples.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void TestCombineStreamsFailsDuringResample()
        {
            const int SourceStream = 100;
            const int MixerStream = 101;
            const int Seconds = 50;
            const int StartAt = 0;

            streamFactory.Setup(f => f.CreateMixerStream(SampleRate)).Returns(MixerStream);
            proxy.Setup(p => p.FreeStream(SourceStream)).Returns(true);
            proxy.Setup(p => p.FreeStream(MixerStream)).Returns(true);
            proxy.Setup(p => p.CombineMixerStreams(MixerStream, SourceStream, BASSFlag.BASS_MIXER_FILTER)).Returns(false);
            proxy.Setup(p => p.GetLastError()).Returns("Combining streams failed");

            resampler.Resample(SourceStream, SampleRate, Seconds, StartAt, mixerStream => new QueueSamplesProvider(new Queue<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void TestSeekToSecondFailedBeforeResample()
        {
            const int SourceStream = 100;
            const int Seconds = 50;
            const int StartAt = 10;

            proxy.Setup(p => p.ChannelSetPosition(SourceStream, StartAt)).Returns(false);
            proxy.Setup(p => p.GetLastError()).Returns("Failed to seek to a specific second");
            proxy.Setup(p => p.FreeStream(SourceStream)).Returns(true);

            resampler.Resample(SourceStream, SampleRate, Seconds, StartAt, mixerStream => new QueueSamplesProvider(new Queue<int>()));
        }
    }
}
