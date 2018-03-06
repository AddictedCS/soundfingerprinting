namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    using Audio;
    using Data;

    using NUnit.Framework;

    public abstract class IntegrationWithSampleFilesTest : AbstractTest
    {
        protected readonly string PathToMp3 = Path.Combine(TestContext.CurrentContext.TestDirectory, "Chopin.mp3");
        protected readonly string PathToSamples = Path.Combine(TestContext.CurrentContext.TestDirectory, "chopinsamples.bin");
        protected readonly string PathToWav = Path.Combine(TestContext.CurrentContext.TestDirectory, "chopin_short.wav");
        protected readonly string PathToChirp = Path.Combine(TestContext.CurrentContext.TestDirectory, "chirp_44.1khz.wav");
        protected readonly string PathToChirp22 = Path.Combine(TestContext.CurrentContext.TestDirectory, "chirp_22.05khz_24bit.wav");
        protected readonly string PathToChirp11 = Path.Combine(TestContext.CurrentContext.TestDirectory, "chirp_11025hz_16bit.wav");

        protected void AssertHashDatasAreTheSame(IList<HashedFingerprint> firstHashDatas, IList<HashedFingerprint> secondHashDatas)
        {
            Assert.AreEqual(firstHashDatas.Count, secondHashDatas.Count);
         
            // hashes are not ordered as parallel computation is involved
            firstHashDatas = SortHashesBySequenceNumber(firstHashDatas);
            secondHashDatas = SortHashesBySequenceNumber(secondHashDatas);

            for (int i = 0; i < firstHashDatas.Count; i++)
            {
                Assert.AreEqual(firstHashDatas[i].SequenceNumber, secondHashDatas[i].SequenceNumber);
                Assert.AreEqual(firstHashDatas[i].StartsAt, secondHashDatas[i].StartsAt, Epsilon);
                CollectionAssert.AreEqual(firstHashDatas[i].HashBins, secondHashDatas[i].HashBins);
            }
        }

        protected TagInfo GetTagInfo()
        {
            return new TagInfo
            {
                Album = "Album",
                AlbumArtist = "AlbumArtist",
                Artist = "Artist",
                Composer = "Composer",
                Duration = 100.2,
                Genre = "Genre",
                IsEmpty = false,
                ISRC = "ISRC",
                Title = "Title",
                Year = 1986
            };
        }

        protected AudioSamples GetAudioSamples()
        {
            lock (this)
            {
                var serializer = new BinaryFormatter();
                using (Stream stream = new FileStream(PathToSamples, FileMode.Open, FileAccess.Read))
                {
                    return (AudioSamples)serializer.Deserialize(stream);
                }
            }
        }

        private List<HashedFingerprint> SortHashesBySequenceNumber(IEnumerable<HashedFingerprint> hashDatasFromFile)
        {
            return hashDatasFromFile.OrderBy(hashData => hashData.SequenceNumber).ToList();
        }
    }
}
