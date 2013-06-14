namespace SoundFingerprinting.Tests.AudioProxies.Tests
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.Bass;

    [TestClass]
    public class BassAudioServiceTest : BaseTest
    {
        [TestMethod]
        public void ReadMonoFromFileTest()
        {
            using (BassAudioService bass = new BassAudioService())
            {
                string tempFile = string.Format(@"{0}\{1}", Path.GetTempPath(), "0.wav");
                bass.RecodeFileToMonoWave(PathToMp3, tempFile, 5512);
                float[] samples = bass.ReadMonoFromFile(PathToMp3, SampleRate);
                FileInfo info = new FileInfo(tempFile);
                long expectedSize = info.Length - WaveHeader;
                long actualSize = samples.Length * (BitsPerSample / 8);
                Assert.AreEqual(expectedSize, actualSize);
            }
        }
    }
}