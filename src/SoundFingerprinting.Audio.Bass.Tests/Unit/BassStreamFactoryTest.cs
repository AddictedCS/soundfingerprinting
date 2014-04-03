namespace SoundFingerprinting.Audio.Bass.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Tests;

    using Un4seen.Bass;

    [TestClass]
    public class BassStreamFactoryTest : AbstractTest
    {
        private IBassStreamFactory streamFactory;

        private Mock<IBassServiceProxy> proxy;

        [TestInitialize]
        public void SetUp()
        {
            proxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);

            streamFactory = new BassStreamFactory(proxy.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            proxy.VerifyAll();
        }

        [TestMethod]
        public void TestCreateStream()
        {
            const int StreamId = 100;
            proxy.Setup(p => p.CreateStream("path-to-audio-file", It.IsAny<BASSFlag>())).Returns(StreamId);

            var result = streamFactory.CreateStream("path-to-audio-file");

            Assert.AreEqual(StreamId, result);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void TestCreateStreamFailed()
        {
            proxy.Setup(p => p.CreateStream("path-to-audio-file", It.IsAny<BASSFlag>())).Returns(0);
            proxy.Setup(p => p.GetLastError()).Returns("Failed to create stream");

            streamFactory.CreateStream("path-to-audio-file");
        }
        
        [TestMethod]
        public void TestCreateMixerStream()
        {
            const int MixerStreamId = 100;
            proxy.Setup(p => p.CreateMixerStream(5512, BassConstants.NumberOfChannels, It.IsAny<BASSFlag>())).Returns(
                MixerStreamId);

            var result = streamFactory.CreateMixerStream(5512);

            Assert.AreEqual(MixerStreamId, result);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void TestCreateMixerStreamFailed()
        {
            proxy.Setup(p => p.CreateMixerStream(5512, BassConstants.NumberOfChannels, It.IsAny<BASSFlag>())).Returns(0);
            proxy.Setup(p => p.GetLastError()).Returns("Failed to create mixer stream");

            streamFactory.CreateMixerStream(5512);
        }

        [TestMethod]
        public void TestCreateStreamFromUrl()
        {
            const int StreamToUrl = 100;
            proxy.Setup(p => p.CreateStreamFromUrl("url-to-streaming-resource", It.IsAny<BASSFlag>())).Returns(
                StreamToUrl);

            var result = streamFactory.CreateStreamFromStreamingUrl("url-to-streaming-resource");

            Assert.AreEqual(StreamToUrl, result);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void TestCreateStreamFromUrlFailed()
        {
            proxy.Setup(p => p.CreateStreamFromUrl("url-to-streaming-resource", It.IsAny<BASSFlag>())).Returns(0);
            proxy.Setup(p => p.GetLastError()).Returns("Failed to create stream to url");

            streamFactory.CreateStreamFromStreamingUrl("url-to-streaming-resource");
        }

        [TestMethod]
        public void TestCreateStreamToMicrophone()
        {
            const int StreamToMicrophone = 100;

            proxy.Setup(p => p.StartRecording(5512, BassConstants.NumberOfChannels, It.IsAny<BASSFlag>())).Returns(
                StreamToMicrophone);

            var result = streamFactory.CreateStreamFromMicrophone(5512);

            Assert.AreEqual(StreamToMicrophone, result);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void TestCreateStreamToMicrophoneFailed()
        {
            proxy.Setup(p => p.StartRecording(5512, BassConstants.NumberOfChannels, It.IsAny<BASSFlag>())).Returns(0);
            proxy.Setup(p => p.GetLastError()).Returns("Failed to create stream to microphone");

            streamFactory.CreateStreamFromMicrophone(5512);
        }
    }
}
