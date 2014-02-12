namespace SoundFingerprinting.Tests.Integration.Audio
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class BassAudioServiceTest : AbstractIntegrationTest
    {
        [TestMethod]
        public void ComparePreStoredSameplesWithCurrentlyReadAudioSamples()
        {
            BinaryFormatter serializer = new BinaryFormatter();

            using (Stream stream = new FileStream(PathToSamples, FileMode.Open, FileAccess.Read))
            {
                float[] samples = (float[])serializer.Deserialize(stream);
                using (BassAudioService bassAudioService = new BassAudioService())
                {
                    float[] readSamples = bassAudioService.ReadMonoFromFile(PathToMp3, SampleRate);
                    Assert.AreEqual(samples.Length, readSamples.Length);
                    for (int i = 0; i < samples.Length; i++)
                    {
                        Assert.IsTrue(Math.Abs(samples[i] - readSamples[i]) < 0.0000001);
                    }
                }
            }
        }

        [TestMethod]
        public void CompareReadingFromASpecificPartOfTheSong()
        {
            const int SecondsToRead = 10;
            const int StartAtSecond = 20;
            const int AcceptedError = 5;
                    
            BinaryFormatter serializer = new BinaryFormatter();

            using (Stream stream = new FileStream(PathToSamples, FileMode.Open, FileAccess.Read))
            {
                float[] samples = (float[])serializer.Deserialize(stream);
                float[] subsetOfSamples = GetSubsetOfSamplesFromFullSong(samples, SecondsToRead, StartAtSecond);

                using (BassAudioService bassAudioService = new BassAudioService())
                {
                    float[] readSamples = bassAudioService.ReadMonoFromFile(PathToMp3, SampleRate, SecondsToRead, StartAtSecond);
                    Assert.AreEqual(subsetOfSamples.Length, readSamples.Length);
                    Assert.IsTrue(Math.Abs(subsetOfSamples.Sum(s => Math.Abs(s)) - readSamples.Sum(s => Math.Abs(s))) < AcceptedError, "Seek is working wrong!");
                }
            }
        }

        [TestMethod]
        public void ReadMonoFromFileTest()
        {
            using (BassAudioService bassAudioService = new BassAudioService())
            {
                string tempFile = string.Format(@"{0}{1}", Path.GetTempPath(), "0.wav");
                bassAudioService.RecodeFileToMonoWave(PathToMp3, tempFile, SampleRate);
                float[] samples = bassAudioService.ReadMonoFromFile(PathToMp3, SampleRate);
                FileInfo info = new FileInfo(tempFile);
                long expectedSize = info.Length - WaveHeader;
                long actualSize = samples.Length * (BitsPerSample / 8);
                Assert.AreEqual(expectedSize, actualSize);
            }
        }

        private float[] GetSubsetOfSamplesFromFullSong(float[] samples, int secondsToRead, int startAtSecond)
        {
            float[] array = new float[SampleRate * secondsToRead];
            Array.Copy(samples, startAtSecond * SampleRate, array, 0, SampleRate * secondsToRead);
            return array;
        }
    }
}
