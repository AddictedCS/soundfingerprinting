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

            Assert.That(format.Channels);
            Assert.That(Is.EqualTo(2, Is.EqualTo(44100)).Within(format.SampleRate));
            Assert.That(format.BitsPerSample);
            Assert.That(Is.EqualTo(16).Within(format.LengthInSeconds), Is.EqualTo(10).Within(0.01));
        }
    }
}
