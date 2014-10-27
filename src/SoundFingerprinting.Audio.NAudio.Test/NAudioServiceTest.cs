namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Protected;

    using global::NAudio.MediaFoundation;

    using global::NAudio.Wave;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class NAudioServiceTest : AbstractTest
    {
        private readonly Mock<INAudioFactory> naudioFactory = new Mock<INAudioFactory>(MockBehavior.Strict);
        private readonly Mock<INAudioSourceReader> sourceReader = new Mock<INAudioSourceReader>(MockBehavior.Strict);

        private NAudioService naudioService;

        [TestInitialize]
        public void SetUp()
        {
            naudioService = new NAudioService(sourceReader.Object, naudioFactory.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            sourceReader.VerifyAll();
            naudioFactory.VerifyAll();
        }

        [TestMethod]
        public void TestSupportedNAudioFormats()
        {
            var supportedFormats = naudioService.SupportedFormats.ToList();

            Assert.IsTrue(supportedFormats.Contains(".mp3"));
            Assert.IsTrue(supportedFormats.Contains(".wav"));
        }

        [TestMethod]
        public void TestReadMonoSamplesFromFile()
        {
            const int SecondsToRead = 10;
            const int StartAt = 10;
            float[] samples = new float[1024];
            sourceReader.Setup(r => r.ReadMonoFromSource("path-to-audio-file", SampleRate, SecondsToRead, StartAt)).
                Returns(samples);

            var result = naudioService.ReadMonoSamplesFromFile("path-to-audio-file", SampleRate, SecondsToRead, StartAt);

            Assert.AreSame(samples, result);
        }

        [TestMethod]
        public void TestRecodeFileToMonoWave()
        {
            Mock<WaveStream> waveStream = new Mock<WaveStream>(MockBehavior.Strict);
            naudioFactory.Setup(factory => factory.GetStream("path-to-audio-file")).Returns(waveStream.Object);
            const int Mono = 1;
            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, Mono);
            waveStream.Setup(stream => stream.WaveFormat).Returns(waveFormat);
            waveStream.Setup(stream => stream.Close());
            Mock<MediaFoundationTransform> resampler = new Mock<MediaFoundationTransform>(
                MockBehavior.Strict, new object[] { waveStream.Object, waveFormat });
            resampler.Protected().Setup("Dispose", new object[] { true });
            naudioFactory.Setup(factory => factory.GetResampler(waveStream.Object, SampleRate, Mono)).Returns(resampler.Object);
            naudioFactory.Setup(factory => factory.CreateWaveFile("path-to-recoded-file", resampler.Object));

            naudioService.RecodeFileToMonoWave("path-to-audio-file", "path-to-recoded-file", SampleRate);
        }       
    }
}
