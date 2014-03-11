namespace SoundFingerprinting.Audio.NAudio.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using global::NAudio.Wave;
    using global::NAudio.Wave.SampleProviders;

    [TestClass]
    public class NAudioSamplesProviderTest
    {
        [TestMethod]
        public void TestGetNextSamplesQueriesStreamCorrectly()
        {
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(5512, 1);
            var waveProvider = new Mock<IWaveProvider>(MockBehavior.Loose);
            waveProvider.Setup(provider => provider.WaveFormat).Returns(waveFormat);
            var spcb = new Mock<WaveToSampleProvider>(MockBehavior.Strict, waveProvider.Object);
            float[] buffer = new float[1024];
            spcb.Setup(provider => provider.Read(buffer, 0, buffer.Length)).Returns(1024);
            
            var naudioSamplesProvider = new NAudioSamplesProvider(spcb.Object);

            int samplesRead = naudioSamplesProvider.GetNextSamples(buffer);
            Assert.AreEqual(1024 * 4, samplesRead);
        }
    }
}
