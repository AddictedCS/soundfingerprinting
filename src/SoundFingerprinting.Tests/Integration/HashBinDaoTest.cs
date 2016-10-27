namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class HashBinDaoTest : AbstractIntegrationTest
    {
        private IHashBinDao hashBinDao;
        private ITrackDao trackDao;
        private ISubFingerprintDao subFingerprintDao;

        [TestInitialize]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            this.hashBinDao = new HashBinDao(ramStorage);
            this.trackDao = new TrackDao(ramStorage);
            this.subFingerprintDao = new SubFingerprintDao(ramStorage);
        }

        [TestMethod]
        public void InsertReadTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = this.trackDao.InsertTrack(track);
            const int NumberOfHashBins = 100;
            var hashedFingerprints = Enumerable.Range(0, NumberOfHashBins).Select(i => new HashedFingerprint(this.GenericSignature, this.GenericHashBuckets, i, i * 0.928));

            this.InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var hashedFingerprintss = this.hashBinDao.ReadHashedFingerprintsByTrackReference(track.TrackReference);
            Assert.AreEqual(NumberOfHashBins, hashedFingerprintss.Count);
        }

        [TestMethod]
        public void SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSongTest()
        {
            const int StaticStride = 5115;
            TagInfo tagInfo = this.GetTagInfo();
            int releaseYear = tagInfo.Year;
            TrackData track = new TrackData(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            var trackReference = this.trackDao.InsertTrack(track);
            var hashedFingerprints = this.FingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3)
                .WithFingerprintConfig(config =>
                {
                    config.SpectrogramConfig.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .UsingServices(this.AudioService)
                .Hash()
                .Result;

            this.InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);
            
            var hashes = this.hashBinDao.ReadHashedFingerprintsByTrackReference(track.TrackReference);
            Assert.AreEqual(hashedFingerprints.Count, hashes.Count);
            foreach (var data in hashes)
            {
                Assert.AreEqual(25, data.HashBins.Length);
            }
        }

        [TestMethod]
        public void ReadByTrackGroupIdWorksAsExpectedTest()
        {
            const int StaticStride = 5115;
            TagInfo tagInfo = this.GetTagInfo();
            int releaseYear = tagInfo.Year;
            TrackData firstTrack = new TrackData(
                tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration)
                { GroupId = "first-group-id" };
            TrackData secondTrack = new TrackData(
                tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration) { GroupId = "second-group-id" };

            var firstTrackReference = this.trackDao.InsertTrack(firstTrack);
            var secondTrackReference = this.trackDao.InsertTrack(secondTrack);

            var hashedFingerprints = this.FingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 20, 0)
                .WithFingerprintConfig(config =>
                {
                    config.SpectrogramConfig.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .UsingServices(this.AudioService)
                .Hash()
                .Result;

            this.InsertHashedFingerprintsForTrack(hashedFingerprints, firstTrackReference);
            this.InsertHashedFingerprintsForTrack(hashedFingerprints, secondTrackReference);

            const int ThresholdVotes = 25;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprintData = this.hashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hashedFingerprint.HashBins, ThresholdVotes, "first-group-id").ToList();

                Assert.IsTrue(subFingerprintData.Count == 1);
                Assert.AreEqual(firstTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = this.hashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(hashedFingerprint.HashBins, ThresholdVotes, "second-group-id").ToList();

                Assert.IsTrue(subFingerprintData.Count == 1);
                Assert.AreEqual(secondTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = this.hashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(hashedFingerprint.HashBins, ThresholdVotes).ToList();
                Assert.AreEqual(2, subFingerprintData.Count);
            }
        }

        [TestMethod]
        public void ReadHashDataByTrackTest()
        {
            TrackData firstTrack = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            var firstTrackReference = this.trackDao.InsertTrack(firstTrack);

            var firstHashData = this.FingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 10, 0)
                .UsingServices(this.AudioService)
                .Hash()
                .Result;

            this.InsertHashedFingerprintsForTrack(firstHashData, firstTrackReference);

            TrackData secondTrack = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            var secondTrackReference = this.trackDao.InsertTrack(secondTrack);

            var secondHashData = this.FingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 20, 10)
                .UsingServices(this.AudioService)
                .Hash()
                .Result;

            this.InsertHashedFingerprintsForTrack(secondHashData, secondTrackReference);

            var resultFirstHashData = this.hashBinDao.ReadHashedFingerprintsByTrackReference(firstTrackReference);
            this.AssertHashDatasAreTheSame(firstHashData, resultFirstHashData);

            IList<HashedFingerprint> resultSecondHashData = this.hashBinDao.ReadHashedFingerprintsByTrackReference(secondTrackReference);
            this.AssertHashDatasAreTheSame(secondHashData, resultSecondHashData);
        }

        private void InsertHashedFingerprintsForTrack(IEnumerable<HashedFingerprint> hashedFingerprints, IModelReference trackReference)
        {
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprintId = this.subFingerprintDao.InsertSubFingerprint(hashedFingerprint.SubFingerprint, hashedFingerprint.SequenceNumber, hashedFingerprint.Timestamp, trackReference);
                this.hashBinDao.InsertHashBins(hashedFingerprint.HashBins, subFingerprintId, trackReference);
            }
        }
    }
}
