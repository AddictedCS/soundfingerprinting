namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

    public abstract class IntegrationWithSampleFilesTest : AbstractTest
    {
        protected const string PathToMp3 = "Kryptonite.mp3";

        protected const string PathToSamples = @"floatsamples.bin";

        protected void AssertHashDatasAreTheSame(IList<HashedFingerprint> firstHashDatas, IList<HashedFingerprint> secondHashDatas)
        {
            Assert.AreEqual(firstHashDatas.Count, secondHashDatas.Count);
         
            // hashes are not ordered as parallel computation is involved
            firstHashDatas = SortHashesByFirstValueOfHashBin(firstHashDatas);
            secondHashDatas = SortHashesByFirstValueOfHashBin(secondHashDatas);

            for (int i = 0; i < firstHashDatas.Count; i++)
            {
                CollectionAssert.AreEqual(firstHashDatas[i].SubFingerprint, secondHashDatas[i].SubFingerprint);
                CollectionAssert.AreEqual(firstHashDatas[i].HashBins, secondHashDatas[i].HashBins);
                Assert.AreEqual(firstHashDatas[i].SequenceNumber, secondHashDatas[i].SequenceNumber);
                Assert.AreEqual(firstHashDatas[i].StartsAt, secondHashDatas[i].StartsAt, Epsilon);
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

        private List<HashedFingerprint> SortHashesByFirstValueOfHashBin(IEnumerable<HashedFingerprint> hashDatasFromFile)
        {
            return hashDatasFromFile.OrderBy(hashData => hashData.SequenceNumber).ToList();
        }
    }
}
