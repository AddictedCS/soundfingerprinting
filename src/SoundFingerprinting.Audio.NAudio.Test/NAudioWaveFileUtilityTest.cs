namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System;
    using System.IO;

    using Moq;
    using Moq.Protected;

    using global::NAudio.MediaFoundation;

    using global::NAudio.Wave;

    using NUnit.Framework;

    [TestFixture]
    public class NAudioWaveFileUtilityTest
    {
        private readonly Random rand = new Random((int)DateTime.Now.Ticks << 4);
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
            using (var stream = new MemoryStream())
            {
                const int Mono = 1;
                var writer = new Mock<WaveFileWriter>(MockBehavior.Loose, stream, WaveFormat.CreateIeeeFloatWaveFormat(5512, Mono));
                naudioFactory.Setup(factory => factory.GetWriter("path-to-audio-file", 5512, Mono))
                                                      .Returns(writer.Object);
                const int SongLengthInFloats = 16;
                float[] samples = GenerateRandomFloatArray(SongLengthInFloats);
                writer.Setup(w => w.Close());

                waveFileUtility.WriteSamplesToFile(samples, 5512, "path-to-audio-file");

                var readSamples = GetWrittenSamplesInStream(stream, SongLengthInFloats);
                CollectionAssert.AreEqual(samples, readSamples);
            }
        }

        [Test]
        public void TestRecodeFileToMonoWave()
        {
            Mock<WaveStream> waveStream = new Mock<WaveStream>(MockBehavior.Strict);
            naudioFactory.Setup(factory => factory.GetStream("path-to-audio-file")).Returns(waveStream.Object);
            const int Mono = 1;
            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(5512, Mono);
            waveStream.Setup(stream => stream.WaveFormat).Returns(waveFormat);
            waveStream.Setup(stream => stream.Close());
            Mock<MediaFoundationTransform> resampler = new Mock<MediaFoundationTransform>(
                MockBehavior.Strict, new object[] { waveStream.Object, waveFormat });
            resampler.Protected().Setup("Dispose", new object[] { true });
            naudioFactory.Setup(factory => factory.GetResampler(waveStream.Object, 5512, Mono, 25)).Returns(resampler.Object);
            naudioFactory.Setup(factory => factory.CreateWaveFile("path-to-recoded-file", resampler.Object));

            waveFileUtility.RecodeFileToMonoWave("path-to-audio-file", "path-to-recoded-file", 5512, 25);
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

        private float[] GenerateRandomFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (float)this.rand.NextDouble() * 32767;
            }

            return result;
        }
    }
}
