namespace SoundFingerprinting.Audio.NAudio.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class NAudioStreamUrlReaderTest : AbstractTest
    {
        private readonly Mock<INAudioSourceReader> sourceReader = new Mock<INAudioSourceReader>(MockBehavior.Strict);

        private NAudioStreamingUrlReader reader;

        [TestInitialize]
        public void SetUp()
        {
            reader = new NAudioStreamingUrlReader(sourceReader.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            sourceReader.VerifyAll();
        }

        [TestMethod]
        public void TestReadMonoSamplesFromFile()
        {
            const int SecondsToRead = 10;
            float[] samples = new float[1024];
            sourceReader.Setup(r => r.ReadMonoFromSource("path-to-streaming-url", SampleRate, SecondsToRead * 2, 0)).Returns(samples);

            var result = reader.ReadMonoSamples("path-to-streaming-url", SampleRate, SecondsToRead);

            Assert.AreEqual(samples.Length / 2, result.Length);
        }
    }
}