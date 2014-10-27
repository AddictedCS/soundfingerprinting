namespace SoundFingerprinting.Audio.Bass.Tests.Unit
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class BassStreamingUrlReaderTest : AbstractTest
    {
        private IStreamingUrlReader streamingUrlReader;

        private Mock<IBassServiceProxy> proxy;
        private Mock<IBassStreamFactory> streamFactory;
        private Mock<IBassResampler> resampler;

        [TestInitialize]
        public void SetUp()
        {
            proxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);
            streamFactory = new Mock<IBassStreamFactory>(MockBehavior.Strict);
            resampler = new Mock<IBassResampler>(MockBehavior.Strict);

            streamingUrlReader = new BassStreamingUrlReader(proxy.Object, streamFactory.Object, resampler.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            proxy.VerifyAll();
            streamFactory.VerifyAll();
            resampler.VerifyAll();
        }

        [TestMethod]
        public void TestReadMonoFromUrl()
        {
            const int StreamId = 100;
            float[] samplesToReturn = new float[1024];
            streamFactory.Setup(f => f.CreateStreamFromStreamingUrl("url-to-streaming-resource")).Returns(StreamId);
            resampler.Setup(r => r.Resample(StreamId, SampleRate, 30, 0, It.IsAny<Func<int, ISamplesProvider>>())).Returns(samplesToReturn);

            var samples = streamingUrlReader.ReadMonoSamples("url-to-streaming-resource", SampleRate, 30);

            Assert.AreEqual(samplesToReturn, samples);
        }
    }
}