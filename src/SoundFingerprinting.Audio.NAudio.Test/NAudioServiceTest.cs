namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class NAudioServiceTest
    {
        private readonly Mock<INAudioSourceReader> sourceReader = new Mock<INAudioSourceReader>(MockBehavior.Strict);

        private NAudioService naudioService;

        [SetUp]
        public void SetUp()
        {
            naudioService = new NAudioService(25, false, null, sourceReader.Object);
        }

        [TearDown]
        public void TearDown()
        {
            sourceReader.VerifyAll();
        }

        [Test]
        public void TestSupportedNAudioFormats()
        {
            var supportedFormats = naudioService.SupportedFormats.ToList();

            Assert.IsTrue(supportedFormats.Contains(".mp3"));
            Assert.IsTrue(supportedFormats.Contains(".wav"));
        }

        [Test]
        public void TestReadMonoSamplesFromFile()
        {
            const int SecondsToRead = 10;
            const int StartAt = 10;
            float[] samples = new float[1024];
            sourceReader.Setup(r => r.ReadMonoFromSource("path-to-audio-file", 5512, SecondsToRead, StartAt, 25)).Returns(samples);

            var result = naudioService.ReadMonoSamplesFromFile("path-to-audio-file", 5512, SecondsToRead, StartAt);

            Assert.AreSame(samples, result.Samples);
        }
    }
}
