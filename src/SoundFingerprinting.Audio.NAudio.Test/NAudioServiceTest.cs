namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class NAudioServiceTest : AbstractTest
    {
        private readonly Mock<INAudioSourceReader> sourceReader = new Mock<INAudioSourceReader>(MockBehavior.Strict);

        private NAudioService naudioService;

        [TestInitialize]
        public void SetUp()
        {
            naudioService = new NAudioService(sourceReader.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            sourceReader.VerifyAll();
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
            sourceReader.Setup(r => r.ReadMonoFromSource("path-to-audio-file", SampleRate, SecondsToRead, StartAt)).Returns(samples);

            var result = naudioService.ReadMonoSamplesFromFile("path-to-audio-file", SampleRate, SecondsToRead, StartAt);

            Assert.AreSame(samples, result.Samples);
        }
    }
}
