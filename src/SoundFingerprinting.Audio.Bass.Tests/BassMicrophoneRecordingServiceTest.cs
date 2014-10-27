namespace SoundFingerprinting.Audio.Bass.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class BassMicrophoneRecordingServiceTest : AbstractTest
    {
        private BassSoundCaptureService soundCaptureService;

        private Mock<IBassServiceProxy> proxy;
        private Mock<IBassStreamFactory> streamFactory;
        private Mock<IBassResampler> resampler;

        [TestInitialize]
        public void SetUp()
        {
            proxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);
            streamFactory = new Mock<IBassStreamFactory>(MockBehavior.Strict);
            resampler = new Mock<IBassResampler>(MockBehavior.Strict);

            soundCaptureService = new BassSoundCaptureService(proxy.Object, streamFactory.Object, resampler.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            proxy.VerifyAll();
            streamFactory.VerifyAll();
            resampler.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(BassException))]
        public void RecordingIsNotSupported()
        {
            const int NoRecordingDevice = -1;
            proxy.Setup(p => p.GetRecordingDevice()).Returns(NoRecordingDevice);

            soundCaptureService.ReadMonoSamples(SampleRate, 10);
        }

        [TestMethod]
        public void TestReadMonoFromMicrophone()
        {
            const int StreamId = 100;
            float[] samplesToReturn = new float[1024];
            streamFactory.Setup(f => f.CreateStreamFromMicrophone(SampleRate)).Returns(StreamId);
            resampler.Setup(r => r.Resample(StreamId, SampleRate, 30, 0, It.IsAny<Func<int, ISamplesProvider>>())).Returns(samplesToReturn);

            var samples = soundCaptureService.ReadMonoSamples(SampleRate, 30);

            Assert.AreEqual(samplesToReturn, samples);
        }
    }
}
