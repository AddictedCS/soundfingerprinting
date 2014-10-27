namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using global::NAudio.Wave;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class NAudioWaveFileUtilityTest : AbstractTest
    {
        private readonly Mock<INAudioFactory> naudioFactory = new Mock<INAudioFactory>(MockBehavior.Strict);

        private NAudioWaveFileUtility waveFileUtility;

        [TestInitialize]
        public void SetUp()
        {
            waveFileUtility = new NAudioWaveFileUtility(naudioFactory.Object);
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

                waveFileUtility.WriteSamplesToFile(samples, SampleRate, "path-to-audio-file");

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
