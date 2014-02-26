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
            var trackReference = TrackDao.InsertTrack(track);
            var hashData = Enumerable.Range(0, 100).Select(i => new HashData(GenericSignature, GenericHashBuckets));
            
            InsertHashDataForTrack(hashData, trackReference);
            
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
            var trackReference = TrackDao.InsertTrack(track);
            var hashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3)
                .WithFingerprintConfig(config =>
                {
                    config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .Hash()
                .Result;
            
            InsertHashDataForTrack(hashData, trackReference);

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

            var firstTrackReference = TrackDao.InsertTrack(firstTrack);
            var secondTrackReference = TrackDao.InsertTrack(secondTrack);

            var firstHashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 20, 0)
                .WithFingerprintConfig(config =>
                {
                    config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .Hash()
                .Result;

            InsertHashDataForTrack(firstHashData, firstTrackReference);
            InsertHashDataForTrack(firstHashData, secondTrackReference);

            const int ThresholdVotes = 25;
            foreach (var hashData in firstHashData)
            {
                var subFingerprintData = HashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hashData.HashBins, ThresholdVotes, "first-group-id").ToList();

                Assert.IsTrue(subFingerprintData.Count == 1);
                Assert.AreEqual(firstTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = HashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hashData.HashBins, ThresholdVotes, "second-group-id").ToList();

                Assert.IsTrue(subFingerprintData.Count == 1);
                Assert.AreEqual(secondTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = HashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(hashData.HashBins, ThresholdVotes).ToList();
                Assert.AreEqual(2, subFingerprintData.Count);
            }
        }

        [TestMethod]
        public void ReadHashDataByTrackTest()
        {
            TrackData firstTrack = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            var firstTrackReference = TrackDao.InsertTrack(firstTrack);

            var firstHashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 10, 0)
                .WithDefaultFingerprintConfig()
                .Hash()
                .Result;

            InsertHashDataForTrack(firstHashData, firstTrackReference);

            TrackData secondTrack = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            var secondTrackReference = TrackDao.InsertTrack(secondTrack);

            var secondHashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 20, 10)
                .WithDefaultFingerprintConfig()
                .Hash()
                .Result;

            InsertHashDataForTrack(secondHashData, secondTrackReference);

            var resultFirstHashData = HashBinDao.ReadHashDataByTrackReference(firstTrackReference);
            AssertHashDatasAreTheSame(firstHashData, resultFirstHashData);

            IList<HashData> resultSecondHashData = HashBinDao.ReadHashDataByTrackReference(secondTrackReference);
            AssertHashDatasAreTheSame(secondHashData, resultSecondHashData);
        }

        private void InsertHashDataForTrack(IEnumerable<HashData> hashData, IModelReference trackReference)
        {
            foreach (var hash in hashData)
            {
                var subFingerprintId = SubFingerprintDao.InsertSubFingerprint(hash.SubFingerprint, trackReference);
                HashBinDao.InsertHashBins(hash.HashBins, subFingerprintId);
            }
        }
    }
}
