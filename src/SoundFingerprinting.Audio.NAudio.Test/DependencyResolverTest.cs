namespace SoundFingerprinting.Audio.NAudio.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.NAudio.Play;

    [TestClass]
    public class DependencyResolverTest
    {
        [TestMethod]
        public void TestPublicInterfacesAreResolvedFromNAudioModule()
        {
            var naudioService = new NAudioService();
            var naudioPlayService = new NAudioPlayAudioFileService();

            Assert.IsNotNull(naudioPlayService);
            Assert.IsNotNull(naudioService);
        }
    }
}
