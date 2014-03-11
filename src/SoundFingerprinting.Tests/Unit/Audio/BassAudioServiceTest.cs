namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Infrastructure;

    using Un4seen.Bass;

    [TestClass]
    public class BassAudioServiceTest : AbstractTest
    {
        private const int DefaultBufferLengthInSeconds = 20;

        private BassAudioService bassAudioService;

        private Mock<IBassServiceProxy> bassServiceProxy;

        [TestInitialize]
        public void SetUp()
        {
            bassServiceProxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);
            DependencyResolver.Current.Bind<IBassServiceProxy, IBassServiceProxy>(bassServiceProxy.Object);
            bassAudioService = new BassAudioService();
        }

        [TestCleanup]
        public void TearDown()
        {
            bassServiceProxy.VerifyAll();
        }

        [TestMethod]
        public void GetRecordingDeviceTest()
        {
            const int RecordingDevice = 123;
            bassServiceProxy.Setup(proxy => proxy.GetRecordingDevice()).Returns(RecordingDevice);

            bool isSupported = bassAudioService.IsRecordingSupported;

            Assert.IsTrue(isSupported);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void ReadMonoFromFileThrowsExceptionInCaseIfNoStreamIsCreated()
        {
            bassServiceProxy.Setup(proxy => proxy.GetLastError()).Returns("Could not create stream from specified path");

            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateStream(
                    "path-to-audio-file",
                    BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT)).Returns(0);

            bassAudioService.ReadMonoFromFile("path-to-audio-file", 5512);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void ReadMonoFromUrlThrowsExceptionInCaseIfNoStreamIsCreated()
        {
            bassServiceProxy.Setup(proxy => proxy.GetLastError()).Returns("Could not create stream from specified path");

            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateStreamFromUrl(
                    "path-to-streaming-url",
                    BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT)).Returns(0);

            bassAudioService.ReadMonoSamplesFromStreamingUrl("path-to-streaming-url", 5512, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void ReadMonoFromFileThrowsExceptionInCaseIfNoMixerStreamIsCreated()
        {
            const int StreamId = 123;

            bassServiceProxy.Setup(proxy => proxy.GetLastError()).Returns("Could not create mixer stream");

            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateStream(
                    "path-to-audio-file",
                    BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT)).Returns(StreamId);
            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateMixerStream(
                    5512, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT))
                    .Returns(0);
            bassServiceProxy.Setup(proxy => proxy.FreeStream(StreamId)).Returns(true);
           
            bassAudioService.ReadMonoFromFile("path-to-audio-file", 5512);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void ReadMonoFromFileThrowsExceptionInCaseIfStreamCannotBeCombined()
        {
            const int StreamId = 123;
            const int MixerStreamId = 124;

            bassServiceProxy.Setup(proxy => proxy.GetLastError()).Returns("Could not combine streams");

            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateStream(
                    "path-to-audio-file",
                    BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT)).Returns(StreamId);
            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateMixerStream(
                    5512, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT))
                    .Returns(MixerStreamId);
            bassServiceProxy.Setup(proxy => proxy.CombineMixerStreams(MixerStreamId, StreamId, BASSFlag.BASS_MIXER_FILTER))
                    .Returns(false);

            bassServiceProxy.Setup(proxy => proxy.FreeStream(StreamId)).Returns(true);
            bassServiceProxy.Setup(proxy => proxy.FreeStream(MixerStreamId)).Returns(true);

            bassAudioService.ReadMonoFromFile("path-to-audio-file", 5512);
        }

        [TestMethod]
        [ExpectedException(typeof(BassAudioServiceException))]
        public void ReadMonoFromFileSeekFailsWithError()
        {
            const int StreamId = 123;
            
            bassServiceProxy.Setup(proxy => proxy.GetLastError()).Returns("Could not seek to specified second");

            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateStream(
                    "path-to-audio-file",
                    BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT)).Returns(StreamId);
            bassServiceProxy.Setup(proxy => proxy.ChannelSetPosition(StreamId, 10)).Returns(false);
            bassServiceProxy.Setup(proxy => proxy.FreeStream(StreamId)).Returns(true);
            
            bassAudioService.ReadMonoFromFile("path-to-audio-file", 5512, 10, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(AudioServiceException))]
        public void ChannelGetDataFailsWithError()
        {
            const int StreamId = 123;
            const int MixerStreamId = 124;

            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateStream(
                    "path-to-audio-file",
                    BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT)).Returns(StreamId);
            bassServiceProxy.Setup(
                proxy =>
                proxy.CreateMixerStream(
                    5512, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT))
                    .Returns(MixerStreamId);
            bassServiceProxy.Setup(proxy => proxy.CombineMixerStreams(MixerStreamId, StreamId, BASSFlag.BASS_MIXER_FILTER))
                    .Returns(true);
            bassServiceProxy.Setup(proxy => proxy.ChannelSetPosition(StreamId, 10)).Returns(true);

            bassServiceProxy.Setup(proxy => proxy.FreeStream(StreamId)).Returns(true);
            bassServiceProxy.Setup(proxy => proxy.FreeStream(MixerStreamId)).Returns(true);

            const int BytesRead = -1;
            bassServiceProxy.Setup(
                proxy =>
                proxy.ChannelGetData(
                    MixerStreamId, It.IsAny<float[]>(), 5512 * 10 * 4))
                    .Returns(BytesRead);

            bassAudioService.ReadMonoFromFile("path-to-audio-file", 5512, 10, 10);
        }

       
    }
}
