namespace SoundFingerprinting.Audio.NAudio.Test
{
    using Moq;
    using Moq.Protected;

    using global::NAudio.Wave;

    using NUnit.Framework;

    [TestFixture]
    [Category("RequiresWindowsDLL")]
    public class NAudioSoundCaptureServiceTest
    {
        private readonly Mock<INAudioFactory> naudioFactory = new Mock<INAudioFactory>(MockBehavior.Strict);
        private readonly Mock<ISamplesAggregator> samplesAggregator = new Mock<ISamplesAggregator>(MockBehavior.Strict);

        private NAudioSoundCaptureService soundCaptureService;

        [SetUp]
        public void SetUp()
        {
            soundCaptureService = new NAudioSoundCaptureService(samplesAggregator.Object, naudioFactory.Object);
        }

        [Test]
        public void ShouldReadFromMicrophone()
        {
            var waveInEvent = new Mock<WaveInEvent>();
            naudioFactory.Setup(factory => factory.GetWaveInEvent(5512, 1)).Returns(waveInEvent.Object);
            float[] samples = new float[1024];
            const int secondsToRecord = 10;
            samplesAggregator.Setup(agg => agg.ReadSamplesFromSource(It.IsAny<ISamplesProvider>(), secondsToRecord, 5512)).Returns(samples);

            float[] resultSamples = soundCaptureService.ReadMonoSamples(5512, secondsToRecord);

            Assert.AreSame(samples, resultSamples);
        }
    }
}
