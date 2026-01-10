namespace SoundFingerprinting.Tests.Integration
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class SoundFingerprintingAudioServiceIntTest : IntegrationWithSampleFilesTest
    {
        [Test]
        public void ShouldEstimateLengthCorrectly()
        {
            var audioService = new SoundFingerprintingAudioService();
                
            var duration = audioService.GetLengthInSeconds(PathToWav);

            Assert.That(duration, Is.EqualTo(10.0f).Within(0.1));
        }
    }
}
