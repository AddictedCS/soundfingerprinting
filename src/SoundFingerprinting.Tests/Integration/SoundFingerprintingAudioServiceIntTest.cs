namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class SoundFingerprintingAudioServiceIntTest : IntegrationWithSampleFilesTest
    {
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

        [Test]
        public void ShouldEstimateLengthCorrectly()
        {
            float duration = audioService.GetLengthInSeconds(PathToWav);

            Assert.AreEqual(10.0f, duration, 0.1);
        }
    }

}
