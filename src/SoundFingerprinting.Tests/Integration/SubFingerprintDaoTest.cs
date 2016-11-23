namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Strides;

    [TestFixture]
    [Category("RequiresWindowsDLL")]
    public class SubFingerprintDaoTest : IntegrationWithSampleFilesTest
    {
        private readonly FingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly IAudioService audioService = new NAudioService();
        private ISubFingerprintDao subFingerprintDao;
        private ITrackDao trackDao;

        [SetUp]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            subFingerprintDao = new SubFingerprintDao(ramStorage);
            trackDao = new TrackDao(ramStorage);
        }

        [Test]
        public void ShouldInsertAndReadSubFingerprints()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = trackDao.InsertTrack(track);
            const int NumberOfHashBins = 100;
            var hashedFingerprints = Enumerable.Range(0, NumberOfHashBins).Select(i => new HashedFingerprint(GenericSignature, GenericHashBuckets, i, i * 0.928));

            InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var hashedFingerprintss = subFingerprintDao.ReadHashedFingerprintsByTrackReference(track.TrackReference);
            Assert.AreEqual(NumberOfHashBins, hashedFingerprintss.Count);
            foreach (var hashedFingerprint in hashedFingerprintss)
            {
                CollectionAssert.AreEqual(GenericHashBuckets, hashedFingerprint.HashBins);
            }
        }

        [Test]
        public void SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSongTest()
        {
            TagInfo tagInfo = GetTagInfo();
            int releaseYear = tagInfo.Year;
            var track = new TrackData(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            var trackReference = trackDao.InsertTrack(track);
            var hashedFingerprints = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3)
                .UsingServices(audioService)
                .Hash()
                .Result;

            InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var hashes = subFingerprintDao.ReadHashedFingerprintsByTrackReference(track.TrackReference);
            Assert.AreEqual(hashedFingerprints.Count, hashes.Count);
            foreach (var data in hashes)
            {
                Assert.AreEqual(25, data.HashBins.Length);
            }
        }

        [Test]
        public void ReadByTrackGroupIdWorksAsExpectedTest()
        {
            const int StaticStride = 5115;
            TagInfo tagInfo = GetTagInfo();
            int releaseYear = tagInfo.Year;
            TrackData firstTrack = new TrackData(
                tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration) { GroupId = "first-group-id" };
            TrackData secondTrack = new TrackData(
                tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration) { GroupId = "second-group-id" };

            var firstTrackReference = trackDao.InsertTrack(firstTrack);
            var secondTrackReference = trackDao.InsertTrack(secondTrack);

            var hashedFingerprints = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 20, 0)
                .WithFingerprintConfig(config =>
                {
                    config.SpectrogramConfig.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .UsingServices(audioService)
                .Hash()
                .Result;

            InsertHashedFingerprintsForTrack(hashedFingerprints, firstTrackReference);
            InsertHashedFingerprintsForTrack(hashedFingerprints, secondTrackReference);

            const int ThresholdVotes = 25;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var subFingerprintData = subFingerprintDao.ReadSubFingerprints(hashedFingerprint.HashBins, ThresholdVotes, "first-group-id").ToList();

                Assert.IsTrue(subFingerprintData.Count == 1);
                Assert.AreEqual(firstTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(hashedFingerprint.HashBins, ThresholdVotes, "second-group-id").ToList();

                Assert.IsTrue(subFingerprintData.Count == 1);
                Assert.AreEqual(secondTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(hashedFingerprint.HashBins, ThresholdVotes).ToList();
                Assert.AreEqual(2, subFingerprintData.Count);
            }
        }

        [Test]
        public void ReadHashDataByTrackTest()
        {
            TrackData firstTrack = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            var firstTrackReference = trackDao.InsertTrack(firstTrack);

            var firstHashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 10, 0)
                .UsingServices(audioService)
                .Hash()
                .Result;

            InsertHashedFingerprintsForTrack(firstHashData, firstTrackReference);

            TrackData secondTrack = new TrackData("isrc", "artist", "title", "album", 2012, 200);

            var secondTrackReference = trackDao.InsertTrack(secondTrack);

            var secondHashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3, 20, 10)
                .UsingServices(audioService)
                .Hash()
                .Result;

            InsertHashedFingerprintsForTrack(secondHashData, secondTrackReference);

            var resultFirstHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(firstTrackReference);
            AssertHashDatasAreTheSame(firstHashData, resultFirstHashData);

            IList<HashedFingerprint> resultSecondHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(secondTrackReference);
            AssertHashDatasAreTheSame(secondHashData, resultSecondHashData);
        }

        private void InsertHashedFingerprintsForTrack(IEnumerable<HashedFingerprint> hashedFingerprints, IModelReference trackReference)
        {
            subFingerprintDao.InsertHashDataForTrack(hashedFingerprints, trackReference);
        }
    }
}
