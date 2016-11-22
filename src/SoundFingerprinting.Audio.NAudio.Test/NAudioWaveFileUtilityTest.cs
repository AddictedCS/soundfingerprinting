namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System.IO;

    using Moq;
    using Moq.Protected;

    using global::NAudio.MediaFoundation;

    using global::NAudio.Wave;

    using NUnit.Framework;

    using SoundFingerprinting.Tests;

    [TestFixture]
    public class NAudioWaveFileUtilityTest : AbstractTest
    {
        private readonly Mock<INAudioFactory> naudioFactory = new Mock<INAudioFactory>(MockBehavior.Strict);

        private NAudioWaveFileUtility waveFileUtility;

        [SetUp]
        public void SetUp()
        {
            waveFileUtility = new NAudioWaveFileUtility(naudioFactory.Object);
        }

        [Test]
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

                waveFileUtility.WriteSamplesToFile(samples, SampleRate, "path-to-audio-file");

                var readSamples = GetWrittenSamplesInStream(memoryStream, SongLengthInFloats);
                AssertArraysAreEqual(samples, readSamples);
            }
        }

        [Test]
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

            waveFileUtility.RecodeFileToMonoWave("path-to-audio-file", "path-to-recoded-file", SampleRate);
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
