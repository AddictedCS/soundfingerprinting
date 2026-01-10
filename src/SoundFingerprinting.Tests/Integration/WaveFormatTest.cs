namespace SoundFingerprinting.Tests.Integration
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using Assert = NUnit.Framework.Legacy.ClassicAssert;
    using static NUnit.Framework.Legacy.ClassicAssert;

    [TestFixture]
    public class WaveFormatTest : IntegrationWithSampleFilesTest
    {
        [Test]
        public void ShouldReadWaveFormatCorrectly()
        {
            var format = WaveFormat.FromFile(PathToWav);

            Assert.AreEqual(2, format.Channels);
            Assert.AreEqual(44100, format.SampleRate);
            Assert.AreEqual(16, format.BitsPerSample);
            Assert.AreEqual(10, format.LengthInSeconds, 0.01);
        }
    }
}
