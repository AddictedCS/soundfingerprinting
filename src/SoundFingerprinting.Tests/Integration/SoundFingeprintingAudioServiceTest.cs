namespace SoundFingerprinting.Tests.Integration
{
    using System.IO;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;

    [TestFixture]
    public class SoundFingeprintingAudioServiceTest : IntegrationWithSampleFilesTest
    {
        private IAudioService service = new SoundFingerprintingAudioService();

        private IAudioService audioService = new NAudioService();

        [Test]
        public void ShouldConvert()
        {
            var samples = service.ReadMonoSamplesFromFile(PathToChirp, 5512);
            NAudioWaveFileUtility nAudioWaveFileUtility = new NAudioWaveFileUtility();
            nAudioWaveFileUtility.WriteSamplesToFile(samples.Samples, 5512, @"c:\hui.wav");


            var samples2 = audioService.ReadMonoSamplesFromFile(PathToChirp, 5512);
            nAudioWaveFileUtility.WriteSamplesToFile(samples2.Samples, 5512, @"c:\hui2.wav");
        }
    }
}
