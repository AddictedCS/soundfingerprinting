namespace SoundFingerprinting.Audio.NAudio.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Protected;

    using global::NAudio.Wave;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class NAudioSoundCaptureServiceTest : AbstractTest
    {
        private readonly Mock<INAudioFactory> naudioFactory = new Mock<INAudioFactory>(MockBehavior.Strict);
        private readonly Mock<ISamplesAggregator> samplesAggregator = new Mock<ISamplesAggregator>(MockBehavior.Strict);

        private NAudioSoundCaptureService soundCaptureService;

        [TestInitialize]
        public void SetUp()
        {
            soundCaptureService = new NAudioSoundCaptureService(samplesAggregator.Object, naudioFactory.Object);
        }

        [TestMethod]
        public void TestReadFromMicrophone()
        {
            var waveInEvent = new Mock<WaveInEvent>(MockBehavior.Strict);
            naudioFactory.Setup(factory => factory.GetWaveInEvent(SampleRate, 1)).Returns(waveInEvent.Object);
            float[] samples = TestUtilities.GenerateRandomFloatArray(1024);
            const int SecondsToRecord = 10;
            samplesAggregator.Setup(agg => agg.ReadSamplesFromSource(It.IsAny<ISamplesProvider>(), SecondsToRecord, SampleRate))
                .Returns(samples);
            waveInEvent.Protected().Setup("Dispose", new object[] { true });

            float[] resultSamples = soundCaptureService.ReadMonoSamples(SampleRate, SecondsToRecord);

            Assert.AreSame(samples, resultSamples);
        }
    }
}
