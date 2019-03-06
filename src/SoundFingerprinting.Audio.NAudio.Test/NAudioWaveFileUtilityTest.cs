﻿namespace SoundFingerprinting.Audio.NAudio.Test
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
                const int mono = 1;
                var writer = new Mock<WaveFileWriter>(MockBehavior.Loose, stream, WaveFormat.CreateIeeeFloatWaveFormat(5512, mono));
                naudioFactory.Setup(factory => factory.GetWriter("path-to-audio-file", 5512, mono))
                                                      .Returns(writer.Object);
                const int songLengthInFloats = 16;
                float[] samples = GenerateRandomFloatArray(songLengthInFloats);
                writer.Setup(w => w.Close());

                waveFileUtility.WriteSamplesToFile(samples, 5512, "path-to-audio-file");

                var readSamples = GetWrittenSamplesInStream(stream, songLengthInFloats);
                CollectionAssert.AreEqual(samples, readSamples);
            }
        }

        [Test]
        public void TestRecodeFileToMonoWave()
        {
            Mock<WaveStream> waveStream = new Mock<WaveStream>(MockBehavior.Strict);
            naudioFactory.Setup(factory => factory.GetStream("path-to-audio-file")).Returns(waveStream.Object);
            const int mono = 1;
            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(5512, mono);
            waveStream.Setup(stream => stream.WaveFormat).Returns(waveFormat);
            waveStream.Setup(stream => stream.Close());
            Mock<MediaFoundationTransform> resampler = new Mock<MediaFoundationTransform>(MockBehavior.Strict, waveStream.Object, waveFormat);
            resampler.Protected().Setup("Dispose", new object[] { true });
            naudioFactory.Setup(factory => factory.GetResampler(waveStream.Object, 5512, mono, 25)).Returns(resampler.Object);
            naudioFactory.Setup(factory => factory.CreateWaveFile("path-to-recoded-file", resampler.Object));

            waveFileUtility.RecodeFileToMonoWave("path-to-audio-file", "path-to-recoded-file", 5512, 25);
        }

        private float[] GetWrittenSamplesInStream(MemoryStream memoryStream, int length)
        {
            const int waveHeaderLength = 58;
            memoryStream.Seek(waveHeaderLength, SeekOrigin.Begin);
            const int bytesInFloat = 4;
            byte[] buffer = new byte[length * bytesInFloat];
            memoryStream.Read(buffer, 0, length * bytesInFloat);
            return SamplesConverter.GetFloatSamplesFromByte(length * bytesInFloat, buffer);
        }

        private float[] GenerateRandomFloatArray(int length)
        {
            float[] result = new float[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (float)rand.NextDouble() * 32767;
            }

            return result;
        }
    }
}
