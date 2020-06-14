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

            Assert.AreEqual(10.0f, duration, 0.1);
        }
    }
}
