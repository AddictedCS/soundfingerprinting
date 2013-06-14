namespace SoundFingerprinting.UnitTests.AudioProxies.Tests
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.DirectSound;

    [TestClass]
    public class DirectSoundTest : BaseTest
    {
        [TestMethod]
        public void ReadMonoFromFileTest()
        {
#pragma warning disable 612,618
            using (DirectSoundAudioService directSoundAudioService = new DirectSoundAudioService())
#pragma warning restore 612,618
            {
                float[] samples = directSoundAudioService.ReadMonoFromFile(PathToWav, SampleRate);
                FileInfo info = new FileInfo(PathToWav);
                long expectedSize = info.Length - WaveHeader;
                long actualSize = samples.Length * (BitsPerSample / 8);
                Assert.AreEqual(expectedSize, actualSize);
            }
        }
    }
}