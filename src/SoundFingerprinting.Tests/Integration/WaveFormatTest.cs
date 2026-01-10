namespace SoundFingerprinting.Tests.Integration
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class WaveFormatTest : IntegrationWithSampleFilesTest
    {
        [Test]
        public void ShouldReadWaveFormatCorrectly()
        {
            var format = WaveFormat.FromFile(PathToWav);

            Assert.That(format.Channels, Is.EqualTo(2));
            Assert.That(format.SampleRate, Is.EqualTo(44100));
            Assert.That(format.BitsPerSample, Is.EqualTo(16));
            Assert.That(format.LengthInSeconds, Is.EqualTo(10).Within(0.01));
        }
    }
}
