// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.AudioProxies;

namespace Soundfingerprinting.UnitTests.AudioProxies.Tests
{
    /// <summary>
    ///   Testing Direct Sound proxy
    /// </summary>
    [TestClass]
    public class DirectSoundTest : BaseTest
    {
        /// <summary>
        ///   Helper method that adds a header to the .wav file
        /// </summary>
        /// <param name = "byteArrayList">Data</param>
        /// <param name = "wave">Wave format</param>
        /// <param name = "fileSize">Total file size</param>
        private void AddWavHeaderFile(ArrayList byteArrayList, WaveFormat wave, int fileSize)
        {
            ASCIIEncoding ascii = new ASCIIEncoding(); /*Encoding Format*/
            byteArrayList.AddRange(ascii.GetBytes("RIFF"));
            byteArrayList.AddRange(BitConverter.GetBytes(fileSize));
            byteArrayList.AddRange(ascii.GetBytes("WAVE"));
            //fmt  chunk
            byteArrayList.AddRange(ascii.GetBytes("fmt "));
            //length of fmt chunk (never changes)
            byteArrayList.AddRange(BitConverter.GetBytes(16));
            //"1" for pcm encoding
            byte[] cArray = new byte[2];
            Array.Copy(BitConverter.GetBytes(1), cArray, 2);
            byteArrayList.AddRange(cArray);
            Array.Copy(BitConverter.GetBytes(wave.Channels), cArray, 2);
            byteArrayList.AddRange(cArray);
            byteArrayList.AddRange(BitConverter.GetBytes(wave.SamplesPerSecond));
            byteArrayList.AddRange(BitConverter.GetBytes(wave.AverageBytesPerSecond));
            Array.Copy(BitConverter.GetBytes(wave.BlockAlign), cArray, 2);
            byteArrayList.AddRange(cArray);
            Array.Copy(BitConverter.GetBytes(wave.BitsPerSample), cArray, 2);
            byteArrayList.AddRange(cArray);
            //the data chunk
            byteArrayList.AddRange(ascii.GetBytes("data"));
            byteArrayList.AddRange(BitConverter.GetBytes(fileSize));
        }

        /// <summary>
        ///   Read Mono data from the file test
        /// </summary>
        [TestMethod]
        public void ReadMonoFromFileTest()
        {
#pragma warning disable 612,618
            using (DirectSoundProxy dsproxy = new DirectSoundProxy())
#pragma warning restore 612,618
            {
                float[] samples = dsproxy.ReadMonoFromFile(PATH_TO_WAV, SAMPLE_RATE);
                FileInfo info = new FileInfo(PATH_TO_WAV);
                long expectedSize = info.Length - WAVE_HEADER;
                long actualSize = samples.Length*(BITS_PER_SAMPLE/8);
                Assert.AreEqual(expectedSize, actualSize);
            }
        }
    }
}