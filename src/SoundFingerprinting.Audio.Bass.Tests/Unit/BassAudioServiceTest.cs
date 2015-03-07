namespace SoundFingerprinting.Audio.Bass.Tests.Unit
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Tests;

    [TestClass]
    public class BassAudioServiceTest : AbstractTest
    {
        private IAudioService audioService;
       
        private Mock<IBassServiceProxy> proxy;
        private Mock<IBassStreamFactory> streamFactory;
        private Mock<IBassResampler> resampler;

        [TestInitialize]
        public void SetUp()
        {
            proxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);
            streamFactory = new Mock<IBassStreamFactory>(MockBehavior.Strict);
            resampler = new Mock<IBassResampler>(MockBehavior.Strict);

            audioService = new BassAudioService(proxy.Object, streamFactory.Object, resampler.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            proxy.VerifyAll();
            streamFactory.VerifyAll();
            resampler.VerifyAll();
        }

        [TestMethod]
        public void TestReadMonoFromFile()
        {
            const int StreamId = 100;
            float[] samplesToReturn = new float[1024];
            streamFactory.Setup(f => f.CreateStream("path-to-file")).Returns(StreamId);
            resampler.Setup(r => r.Resample(StreamId, SampleRate, 0, 0, It.IsAny<Func<int, ISamplesProvider>>()))
                .Returns(samplesToReturn);

            var samples = audioService.ReadMonoSamplesFromFile("path-to-file", SampleRate);

            Assert.AreSame(samplesToReturn, samples.Samples);
        }

        [TestMethod]
        public void TestReadMonoFromFileFromSpecificSecond()
        {
            const int StreamId = 100;
            float[] samplesToReturn = new float[1024];
            streamFactory.Setup(f => f.CreateStream("path-to-file")).Returns(StreamId);
            resampler.Setup(r => r.Resample(StreamId, SampleRate, 10, 20, It.IsAny<Func<int, ISamplesProvider>>())).Returns(samplesToReturn);

            var samples = audioService.ReadMonoSamplesFromFile("path-to-file", SampleRate, 10, 20);

            Assert.AreSame(samplesToReturn, samples.Samples);
        }
    }
}
