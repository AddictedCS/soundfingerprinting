namespace SoundFingerprinting.Audio.NAudio.Test
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio.NAudio.Play;

    [TestFixture]
    public class DependencyResolverTest
    {
        [Test]
        public void PublicInterfacesAreResolvedFromNAudioModule()
        {
            var audioService = new NAudioService();
            var audioPlayAudioFileService = new NAudioPlayAudioFileService();

            Assert.IsNotNull(audioPlayAudioFileService);
            Assert.IsNotNull(audioService);
        }
    }
}
