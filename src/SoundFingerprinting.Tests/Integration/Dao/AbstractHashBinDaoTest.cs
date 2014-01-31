namespace SoundFingerprinting.Tests.Integration.Dao
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    [TestClass]
    public abstract class AbstractHashBinDaoTest : AbstractIntegrationTest
    {
        private IFingerprintCommandBuilder fingerprintCommandBuilder;
        private ITagService tagService;

        public abstract IHashBinDao HashBinDao { get; set; }

        public abstract ITrackDao TrackDao { get; set; }

        public abstract ISubFingerprintDao SubFingerprintDao { get; set; }

        [TestInitialize]
        public override void SetUp()
        {
            base.SetUp();
            fingerprintCommandBuilder = new FingerprintCommandBuilder();
            tagService = DependencyResolver.Current.Get<ITagService>();
        }

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
        public void SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSongTest()
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
        public void ReadByTrackGroupIdWorksAsExpectedTest()
        {
            const int StaticStride = 5115;
            TagInfo tagInfo = tagService.GetTagInfo(PathToMp3);
            int releaseYear = tagInfo.Year;
            TrackData firstTrack = new TrackData(
                tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration)
                { GroupId = "first-group-id" };
            TrackData secondTrack = new TrackData(
                tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration) { GroupId = "second-group-id" };

            int firstTrackId = TrackDao.Insert(firstTrack);
            int secondTrackId = TrackDao.Insert(secondTrack);
            var firstHashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 20, 0)
                .WithFingerprintConfig(config =>
                {
                    config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .Hash()
                .Result;

            foreach (var hash in firstHashData)
            {
                long subFingerprintId = SubFingerprintDao.Insert(hash.SubFingerprint, firstTrackId);
                HashBinDao.Insert(hash.HashBins, subFingerprintId);

                subFingerprintId = SubFingerprintDao.Insert(hash.SubFingerprint, secondTrackId);
                HashBinDao.Insert(hash.HashBins, subFingerprintId);
            }

            foreach (var hashData in firstHashData)
            {
                var subFingerprintData = HashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hashData.HashBins, 25, "first-group-id").ToList();

                Assert.IsTrue(subFingerprintData.Count == 1);
                Assert.AreEqual(firstTrackId, subFingerprintData[0].TrackReference.HashCode);

                subFingerprintData = HashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hashData.HashBins, 25, "second-group-id").ToList();

                Assert.IsTrue(subFingerprintData.Count == 1);
                Assert.AreEqual(secondTrackId, subFingerprintData[0].TrackReference.HashCode);

                subFingerprintData = HashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(hashData.HashBins, 25).ToList();
                Assert.AreEqual(2, subFingerprintData.Count);
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

            var resultFirstHashData = HashBinDao.ReadHashDataByTrackId(firstTrackId);
            AssertHashDatasAreTheSame(firstHashData, resultFirstHashData);

            IList<HashData> resultSecondHashData = HashBinDao.ReadHashDataByTrackId(secondTrackId);
            AssertHashDatasAreTheSame(secondHashData, resultSecondHashData);
        }
    }
}
