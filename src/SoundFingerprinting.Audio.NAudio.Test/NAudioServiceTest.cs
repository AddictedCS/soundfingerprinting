namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System.IO;
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

        private readonly Mock<ISamplesAggregator> samplesAggregator = new Mock<ISamplesAggregator>(MockBehavior.Strict);

        private NAudioService naudioService;

        [TestInitialize]
        public void SetUp()
        {
            naudioService = new NAudioService(samplesAggregator.Object, naudioFactory.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            samplesAggregator.VerifyAll();
            naudioFactory.VerifyAll();
        }

        [TestMethod]
        public void TestIsRecordingSupported()
        {
            bool isSupported = naudioService.IsRecordingSupported;

            Assert.IsTrue(isSupported);
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
            Mock<WaveStream> waveStream = new Mock<WaveStream>(MockBehavior.Strict);
            naudioFactory.Setup(factory => factory.GetStream("path-to-audio-file")).Returns(waveStream.Object);
            const int Mono = 1;
            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, Mono);
            waveStream.Setup(stream => stream.WaveFormat).Returns(waveFormat);
            waveStream.Setup(stream => stream.Close());
            const int StartAt = 20;
            waveStream.Setup(stream => stream.Seek(SampleRate * waveFormat.BitsPerSample / 8 * StartAt, SeekOrigin.Begin))
                .Returns(440960);
            Mock<MediaFoundationTransform> resampler = new Mock<MediaFoundationTransform>(
                MockBehavior.Strict, new object[] { waveStream.Object, waveFormat });
            resampler.Protected().Setup("Dispose", new object[] { true });
            naudioFactory.Setup(factory => factory.GetResampler(waveStream.Object, SampleRate, Mono)).Returns(resampler.Object);
            float[] samplesArray = TestUtilities.GenerateRandomFloatArray(1024);
            const int SecondsToRead = 10;
            samplesAggregator.Setup(
                agg => agg.ReadSamplesFromSource(It.IsAny<NAudioSamplesProviderAdapter>(), SecondsToRead, SampleRate)).Returns(
                    samplesArray);

            var result = naudioService.ReadMonoSamplesFromFile("path-to-audio-file", SampleRate, SecondsToRead, StartAt);

            Assert.AreSame(samplesArray, result);
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

        [TestMethod]
        public void TestWriteSamplesToWaveFile()
        {
            using (var memoryStream = new MemoryStream())
            {
                const int Mono = 1;
                Mock<WaveFileWriter> writer = new Mock<WaveFileWriter>(
                    MockBehavior.Strict, memoryStream, WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, Mono));
                naudioFactory.Setup(factory => factory.GetWriter("path-to-audio-file", SampleRate, Mono)).Returns(
                    writer.Object);
                const int SongLengthInFloats = 16;
                float[] samples = TestUtilities.GenerateRandomFloatArray(SongLengthInFloats);
                writer.Setup(w => w.Close());

                naudioService.WriteSamplesToWaveFile("path-to-audio-file", samples, SampleRate);

                var readSamples = GetWrittenSamplesInStream(memoryStream, SongLengthInFloats);
                AssertArraysAreEqual(samples, readSamples);
            }
        }

        private float[] GetWrittenSamplesInStream(MemoryStream memoryStream, int length)
        {
            const int WaveHeaderLength = 58;
            memoryStream.Seek(WaveHeaderLength, SeekOrigin.Begin);
            const int BytesInFloat = 4;
            byte[] buffer = new byte[length * BytesInFloat];
            memoryStream.Read(buffer, 0, length * BytesInFloat);
            return SamplesConverter.GetFloatSamplesFromByte(length * BytesInFloat, buffer);
        }

        private void AssertArraysAreEqual(float[] samples, float[] readSamples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                Assert.AreEqual(samples[i], readSamples[i]);
            }
        }
    }
}
