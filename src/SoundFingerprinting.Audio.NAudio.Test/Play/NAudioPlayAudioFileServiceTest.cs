namespace SoundFingerprinting.Audio.NAudio.Test.Play
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using global::NAudio.Wave;

    using SoundFingerprinting.Audio.NAudio.Play;

    [TestClass]
    public class NAudioPlayAudioFileServiceTest
    {
        private NAudioPlayAudioFileService service;

        private Mock<INAudioPlayAudioFactory> factory;

        [TestInitialize]
        public void SetUp()
        {
            factory = new Mock<INAudioPlayAudioFactory>(MockBehavior.Strict);
            service = new NAudioPlayAudioFileService(factory.Object);
        }

        [TestMethod]
        public void TestPlayFile()
        {
            Mock<IWavePlayer> wavePlayer = new Mock<IWavePlayer>(MockBehavior.Strict);
            factory.Setup(f => f.CreateNewWavePlayer()).Returns(wavePlayer.Object);
            Mock<TestWaveStream> waveStream = new Mock<TestWaveStream>(MockBehavior.Loose);
            factory.Setup(f => f.CreateNewStreamFromFilename("path-to-file")).Returns(waveStream.Object);
            wavePlayer.Setup(w => w.Init(waveStream.Object));
            wavePlayer.Setup(w => w.Play());

            var playFileAttributes = service.PlayFile("path-to-file");

            Assert.IsInstanceOfType(playFileAttributes, typeof(PlayFileAttributes));
            var attributes = playFileAttributes as PlayFileAttributes;
            if (attributes != null)
            {
                Assert.AreSame(wavePlayer.Object, attributes.WavePlayer);
                Assert.AreSame(waveStream.Object, attributes.WaveStream);
            }
        }

        [TestMethod]
        public void TestStopPlayingFile()
        {
            Mock<IWavePlayer> wavePlayer = new Mock<IWavePlayer>(MockBehavior.Strict);
            factory.Setup(f => f.CreateNewWavePlayer()).Returns(wavePlayer.Object);
            Mock<TestWaveStream> waveStream = new Mock<TestWaveStream>(MockBehavior.Strict);
            factory.Setup(f => f.CreateNewStreamFromFilename("path-to-file")).Returns(waveStream.Object);
            wavePlayer.Setup(w => w.Init(waveStream.Object));
            wavePlayer.Setup(w => w.Play());
            wavePlayer.Setup(w => w.Stop());
            wavePlayer.Setup(w => w.Dispose());
            waveStream.Setup(w => w.Close());

            var playFileAttributes = service.PlayFile("path-to-file");
            
            service.StopPlayingFile(playFileAttributes);
        }
    }
}
