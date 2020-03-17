namespace SoundFingerprinting.Audio.NAudio.Test
{
    using Moq;

    using global::NAudio.Wave;

    using global::NAudio.Wave.SampleProviders;

    using NUnit.Framework;

    [TestFixture]
    public class NAudioSamplesProviderAdapterTest
    {
        [Test]
        public void TestGetNextSamplesQueriesStreamCorrectly()
        {
            var waveProvider = new Mock<IWaveProvider>(MockBehavior.Loose);
            waveProvider.Setup(provider => provider.WaveFormat).Returns(WaveFormat.CreateIeeeFloatWaveFormat(5512, 1));
            var waveToSampleProvider = new Mock<WaveToSampleProvider>(MockBehavior.Strict, waveProvider.Object);
            const int numberOfReadSamples = 1024;
            float[] buffer = new float[numberOfReadSamples];
            waveToSampleProvider.Setup(provider => provider.Read(buffer, 0, buffer.Length)).Returns(numberOfReadSamples);
            var nAudioSamplesProvider = new NAudioSamplesProviderAdapter(waveToSampleProvider.Object);

            int samplesRead = nAudioSamplesProvider.GetNextSamples(buffer);
            
            Assert.AreEqual(numberOfReadSamples * 4, samplesRead);
        }
    }
}
