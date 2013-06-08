namespace Soundfingerprinting.UnitTests.AudioProxies.Tests
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Audio.Bass;
    using Soundfingerprinting.Audio.DirectSound;

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

        [TestMethod]
        public void ReadMonoFromFileUsingBothProxiesTest()
        {
            var bassAudioService = new BassAudioService();

            #pragma warning disable 612,618
            var directSoundAudioService = new DirectSoundAudioService();
            #pragma warning restore 612,618

            float[] bdata = bassAudioService.ReadMonoFromFile(PathToMp3, 5512);
            float[] ddata = directSoundAudioService.ReadMonoFromFile(PathToWav, 5512);

            for (int i = 0; i < bdata.Length; i++)
            {
                if (Math.Abs(bdata[i] - ddata[i]) > 1)
                {
                    Assert.Fail("Data arrays are different: " + bdata[i] + ":" + ddata[i] + " at " + i);
                }
            }

            Assert.AreEqual(bdata.Length, ddata.Length);

            bassAudioService.Dispose();
            directSoundAudioService.Dispose();
        }
    }
}