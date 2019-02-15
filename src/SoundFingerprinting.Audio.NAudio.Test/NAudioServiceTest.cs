namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class NAudioServiceTest
    {
        private readonly Mock<INAudioSourceReader> sourceReader = new Mock<INAudioSourceReader>(MockBehavior.Strict);

        private NAudioService nAudioService;

        [SetUp]
        public void SetUp()
        {
            nAudioService = new NAudioService(25, false, null, sourceReader.Object);
        }

        [TearDown]
        public void TearDown()
        {
            sourceReader.VerifyAll();
        }

        [Test]
        public void TestSupportedNAudioFormats()
        {
            var supportedFormats = nAudioService.SupportedFormats.ToList();

            Assert.IsTrue(supportedFormats.Contains(".mp3"));
            Assert.IsTrue(supportedFormats.Contains(".wav"));
        }

        [Test]
        public void TestReadMonoSamplesFromFile()
        {
            const int secondsToRead = 10;
            const int startAt = 10;
            float[] samples = new float[1024];
            sourceReader.Setup(r => r.ReadMonoFromSource("path-to-audio-file", 5512, secondsToRead, startAt, 25)).Returns(samples);

            var result = nAudioService.ReadMonoSamplesFromFile("path-to-audio-file", 5512, secondsToRead, startAt);

            Assert.AreSame(samples, result.Samples);
        }
    }
}
