namespace SoundFingerprinting.Audio.NAudio.Test
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio.NAudio.Play;

    [TestFixture]
    public class DependencyResolverTest
    {
        [Test]
        public void TestPublicInterfacesAreResolvedFromNAudioModule()
        {
            var naudioService = new NAudioService();
            var naudioPlayService = new NAudioPlayAudioFileService();

            Assert.IsNotNull(naudioPlayService);
            Assert.IsNotNull(naudioService);
        }
    }
}
