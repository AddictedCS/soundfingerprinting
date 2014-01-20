namespace SoundFingerprinting.Tests.Integration.Dao
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    public abstract class AbstractHashBinDaoTest : AbstractIntegrationTest
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly ITagService tagService;

        protected AbstractHashBinDaoTest()
        {
            fingerprintCommandBuilder = new FingerprintCommandBuilder();
            tagService = DependencyResolver.Current.Get<ITagService>();
        }

        public abstract IHashBinDao HashBinDao { get; set; }

        public abstract ITrackDao TrackDao { get; set; }

        public abstract ISubFingerprintDao SubFingerprintDao { get; set; }

        [TestMethod]
        public void InsertReadTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = TrackDao.Insert(track);

            for (int i = 0; i < 100; i++)
            {
                long subFingerprintId = SubFingerprintDao.Insert(GenericSignature, trackId);
                HashBinDao.Insert(GenericHashBuckets, subFingerprintId);
            }

            for (int hashTable = 1; hashTable <= GenericHashBuckets.Length; hashTable++)
            {
                var hashBins = HashBinDao.ReadHashBinsByHashTable(hashTable);
                Assert.AreEqual(100, hashBins.Count);
                Assert.AreEqual(GenericHashBuckets[hashTable - 1], hashBins[0].HashBin);
            }
        }

        [TestMethod]
        public void SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSong()
        {
            const int StaticStride = 5115;
            TagInfo tagInfo = tagService.GetTagInfo(PathToMp3);
            int releaseYear = tagInfo.Year;
            TrackData track = new TrackData(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            int trackId = TrackDao.Insert(track);
            var hashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3)
                .WithFingerprintConfig(config =>
                {
                    config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .Hash()
                .Result;

            foreach (var hash in hashData)
            {
                long subFingerprintId = SubFingerprintDao.Insert(hash.SubFingerprint, trackId);
                HashBinDao.Insert(hash.HashBins, subFingerprintId);
            }

            for (int hashTable = 1; hashTable <= 25; hashTable++)
            {
                var hashBins = HashBinDao.ReadHashBinsByHashTable(hashTable);
                Assert.AreEqual(hashData.Count, hashBins.Count);
            }
        }

        [TestMethod]
        public void ReadHashDataByTrackTest()
        {
            TrackData firstTrack = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            int firstTrackId = TrackDao.Insert(firstTrack);

            var firstHashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 10, 0)
                .WithDefaultFingerprintConfig()
                .Hash()
                .Result;

            foreach (var hash in firstHashData)
            {
                long subFingerprintId = SubFingerprintDao.Insert(hash.SubFingerprint, firstTrackId);
                HashBinDao.Insert(hash.HashBins, subFingerprintId);
            }

            TrackData secondTrack = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            int secondTrackId = TrackDao.Insert(secondTrack);

            var secondHashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 20, 10)
                .WithDefaultFingerprintConfig()
                .Hash()
                .Result;

            foreach (var hash in secondHashData)
            {
                long subFingerprintId = SubFingerprintDao.Insert(hash.SubFingerprint, secondTrackId);
                HashBinDao.Insert(hash.HashBins, subFingerprintId);
            }

            IList<HashData> resultFirstHashData = HashBinDao.ReadHashDataByTrackId(firstTrackId);
            AssertHashDatasAreEqual(firstHashData, resultFirstHashData);

            IList<HashData> resultSecondHashData = HashBinDao.ReadHashDataByTrackId(secondTrackId);
            AssertHashDatasAreEqual(secondHashData, resultSecondHashData);
        }

        private static void AssertHashDatasAreEqual(List<HashData> firstHashData, IList<HashData> resultFirstHashData)
        {
            Assert.AreEqual(firstHashData.Count, resultFirstHashData.Count);
            for (int i = 0; i < firstHashData.Count; i++)
            {
                for (int j = 0; j < firstHashData[i].HashBins.Length; j++)
                {
                    Assert.AreEqual(firstHashData[i].HashBins[j], resultFirstHashData[i].HashBins[j]);
                }

                for (int j = 0; j < firstHashData[i].SubFingerprint.Length; j++)
                {
                    Assert.AreEqual(firstHashData[i].SubFingerprint[j], resultFirstHashData[i].SubFingerprint[j]);
                }
            }
        }
    }
}
