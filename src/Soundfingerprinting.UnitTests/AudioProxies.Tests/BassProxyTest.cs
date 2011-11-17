// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.AudioProxies;
using Un4seen.Bass.AddOn.Tags;

namespace Soundfingerprinting.UnitTests.AudioProxies.Tests
{
    /// <summary>
    ///   Bass proxy test class
    /// </summary>
    [TestClass]
    public class BassProxyTest : BaseTest
    {
        /// <summary>
        ///   Read tags test
        /// </summary>
        [TestMethod]
        public void GetTagInfoFromFileTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);
            //using BassProxy read tags from file
            using (BassProxy proxy = new BassProxy())
            {
                TAG_INFO tags = proxy.GetTagInfoFromFile(Path.GetFullPath(PATH_TO_MP3));
                Assert.IsNotNull(tags);
                Assert.IsNotNull(tags.artist);
                Assert.IsNotNull(tags.title);
                Assert.IsNotNull(tags.duration);
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read Mono data from the file test
        /// </summary>
        [TestMethod]
        public void ReadMonoFromFileTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            using (BassProxy bass = new BassProxy())
            {
                string tempFile = Path.GetTempPath() + "\\" + 0 + ".wav";
                bass.RecodeTheFile(PATH_TO_MP3, tempFile, 5512);
                float[] samples = bass.ReadMonoFromFile(PATH_TO_MP3, SAMPLE_RATE);
                FileInfo info = new FileInfo(tempFile);
                long expectedSize = info.Length - WAVE_HEADER;
                long actualSize = samples.Length*(BITS_PER_SAMPLE/8);
                Assert.AreEqual(expectedSize, actualSize);
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Reading mono from file using both proxies test
        /// </summary>
        /// <remarks>
        ///   Both paths to the audio files should come up from the same resource
        /// </remarks>
        [TestMethod]
        public void ReadMonoFromFileUsingBothProxiesTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            using (BassProxy bProxy = new BassProxy())
            {
#pragma warning disable 612,618
                using (DirectSoundProxy dProxy = new DirectSoundProxy())
#pragma warning restore 612,618
                {
                    float[] bdata = bProxy.ReadMonoFromFile(PATH_TO_MP3, 5512);
                    float[] ddata = dProxy.ReadMonoFromFile(PATH_TO_WAV, 5512);

                    for (int i = 0; i < bdata.Length; i++)
                        if (Math.Abs(bdata[i] - ddata[i]/(Int32.MaxValue)) > 1)
                        {
                            Debugger.Break();
                            Assert.Fail("Data arrays are different: " + bdata[i] + ":" + ddata[i] + " at " + i);
                        }
                    Assert.AreEqual(bdata.Length, ddata.Length);
                }
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}