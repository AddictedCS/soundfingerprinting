namespace Soundfingerprinting.UnitTests.AudioProxies.Tests
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Audio.Services;

    [TestClass]
    public class BassAudioServiceTest : BaseTest
    {
        [TestMethod]
        public void ReadMonoFromFileTest()
        {
            using (BassAudioService bass = new BassAudioService())
            {
                string tempFile = Path.GetTempPath() + "\\" + 0 + ".wav";
                bass.RecodeTheFile(PathToMp3, tempFile, 5512);
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
            using (BassAudioService bassAudioService = new BassAudioService())
            {
                using (DirectSoundAudioService directSoundAudioService = new DirectSoundAudioService())
                {
                    float[] bdata = bassAudioService.ReadMonoFromFile(PathToMp3, 5512);
                    float[] ddata = directSoundAudioService.ReadMonoFromFile(PathToWav, 5512);

                    for (int i = 0; i < bdata.Length; i++)
                    {
                        if ((Math.Abs(bdata[i] - ddata[i]) / int.MaxValue) > 1)
                        {
                            Assert.Fail("Data arrays are different: " + bdata[i] + ":" + ddata[i] + " at " + i);
                        }
                    }

                    Assert.AreEqual(bdata.Length, ddata.Length);
                }
            }
        }
    }
}