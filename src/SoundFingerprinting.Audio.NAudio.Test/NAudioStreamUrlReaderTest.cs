namespace SoundFingerprinting.Audio.NAudio.Test
{
    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class NAudioStreamUrlReaderTest
    {
        private readonly Mock<INAudioSourceReader> sourceReader = new Mock<INAudioSourceReader>(MockBehavior.Strict);

        private NAudioStreamingUrlReader reader;

        [SetUp]
        public void SetUp()
        {
            reader = new NAudioStreamingUrlReader(sourceReader.Object);
        }

        [TearDown]
        public void TearDown()
        {
            sourceReader.VerifyAll();
        }

        [Test]
        public void TestReadMonoSamplesFromFile()
        {
            const int secondsToRead = 10;
            float[] samples = new float[1024];
            sourceReader.Setup(r => r.ReadMonoFromSource("path-to-streaming-url", 5512, secondsToRead * 2, 0, 25)).Returns(samples);

            var result = reader.ReadMonoSamples("path-to-streaming-url", 5512, secondsToRead);

            Assert.AreEqual(samples.Length / 2, result.Length);
        }
    }
}