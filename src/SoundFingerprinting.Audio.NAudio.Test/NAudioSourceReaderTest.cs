namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System;

    using Moq;
    using Moq.Protected;

    using global::NAudio.MediaFoundation;

    using global::NAudio.Wave;

    using NUnit.Framework;

    [TestFixture]
    public class NAudioSourceReaderTest
    {
        private readonly Mock<INAudioFactory> naudioFactory = new Mock<INAudioFactory>(MockBehavior.Strict);
        private readonly Mock<ISamplesAggregator> samplesAggregator = new Mock<ISamplesAggregator>(MockBehavior.Strict);

        private NAudioSourceReader sourceReader;

        [SetUp]
        public void SetUp()
        {
            sourceReader = new NAudioSourceReader(samplesAggregator.Object, naudioFactory.Object);
        }

        [TearDown]
        public void TearDown()
        {
            samplesAggregator.VerifyAll();
            naudioFactory.VerifyAll();
        }

        [Test]
        public void TestReadMonoSamplesFromFile()
        {
            var waveStream = new Mock<WaveStream>(MockBehavior.Strict);
            naudioFactory.Setup(factory => factory.GetStream("path-to-audio-file")).Returns(waveStream.Object);
            const int Mono = 1;
            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(5512, Mono);
            waveStream.Setup(stream => stream.WaveFormat).Returns(waveFormat);
            waveStream.Setup(stream => stream.TotalTime).Returns(TimeSpan.FromSeconds(120));
            waveStream.Setup(stream => stream.Close());
            const double StartAt = 20d;
            waveStream.Setup(stream => stream.CurrentTime).Returns(new TimeSpan());
            waveStream.SetupSet(stream => stream.CurrentTime = TimeSpan.FromSeconds(StartAt));
            var resampler = new Mock<MediaFoundationTransform>(MockBehavior.Strict, new object[] { waveStream.Object, waveFormat });
            resampler.Protected().Setup("Dispose", new object[] { true });
            naudioFactory.Setup(factory => factory.GetResampler(waveStream.Object, 5512, Mono, 25)).Returns(resampler.Object);
            float[] samplesArray = new float[1024];
            const double SecondsToRead = 10d;
            samplesAggregator.Setup(agg => agg.ReadSamplesFromSource(It.IsAny<NAudioSamplesProviderAdapter>(), SecondsToRead, 5512))
                                              .Returns(samplesArray);

            var result = sourceReader.ReadMonoFromSource("path-to-audio-file", 5512, SecondsToRead, StartAt, 25);

            Assert.AreSame(samplesArray, result);
        }
    }
}