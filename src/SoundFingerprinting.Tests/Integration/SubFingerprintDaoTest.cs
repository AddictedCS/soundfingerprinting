namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Math;

    [TestFixture]
    public class SubFingerprintDaoTest : IntegrationWithSampleFilesTest
    {
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();
        private ISubFingerprintDao subFingerprintDao;
        private ITrackDao trackDao;

        [SetUp]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            subFingerprintDao = new SubFingerprintDao(ramStorage, new StandardGroupingCounter());
            trackDao = new TrackDao(ramStorage);
        }

        [Test]
        public void ShouldInsertAndReadSubFingerprints()
        {
            var track = new TrackInfo("isrc", "artist", "title", 200);
            var trackReference = trackDao.InsertTrack(track).TrackReference;
            const int NumberOfHashBins = 100;
            var genericHashBuckets = GenericHashBuckets();
            var hashedFingerprints = Enumerable.Range(0, NumberOfHashBins)
                    .Select(
                        sequenceNumber =>
                            new HashedFingerprint(
                                genericHashBuckets,
                                (uint)sequenceNumber,
                                sequenceNumber * 0.928f,
                                Enumerable.Empty<string>()));

            InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var hashedFingerprintss = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference);
            Assert.AreEqual(NumberOfHashBins, hashedFingerprintss.Count);
            foreach (var hashedFingerprint in hashedFingerprintss)
            {
                CollectionAssert.AreEqual(genericHashBuckets, hashedFingerprint.HashBins);
            }
        }

        [Test]
        public void SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSongTest()
        {
            var track = new TrackInfo("isrc", "artist", "title", 120d);
            var trackReference = trackDao.InsertTrack(track).TrackReference;
            var hashedFingerprints = FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash()
                .Result;

            InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var hashes = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference);
            Assert.AreEqual(hashedFingerprints.Count, hashes.Count);
            foreach (var data in hashes)
            {
                Assert.AreEqual(25, data.HashBins.Length);
            }
        }

        [Test]
        public void ReadByTrackGroupIdWorksAsExpectedTest()
        {
            var firstTrack = new TrackInfo("isrc", "artist", "title", 120d);
            var secondTrack = new TrackInfo("isrc", "artist", "title", 120d);

            var firstTrackReference = trackDao.InsertTrack(firstTrack).TrackReference;
            var secondTrackReference = trackDao.InsertTrack(secondTrack).TrackReference;

            var hashedFingerprintsForFirstTrack = FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .WithFingerprintConfig(config =>
                {
                    config.Clusters = new[] { "first-group-id" };
                    return config;
                })
                .UsingServices(audioService)
                .Hash()
                .Result;

            InsertHashedFingerprintsForTrack(hashedFingerprintsForFirstTrack, firstTrackReference);

            var hashedFingerprintsForSecondTrack = FingerprintCommandBuilder.Instance
               .BuildFingerprintCommand()
               .From(GetAudioSamples())
               .WithFingerprintConfig(config =>
               {
                   config.Clusters = new[] { "second-group-id" };
                   return config;
               })
               .UsingServices(audioService)
               .Hash()
               .Result;
            InsertHashedFingerprintsForTrack(hashedFingerprintsForSecondTrack, secondTrackReference);

            const int ThresholdVotes = 25;

            foreach (var hashedFingerprint in hashedFingerprintsForFirstTrack)
            {
                var subFingerprintData = subFingerprintDao.ReadSubFingerprints(
                    new[] { new QueryHash(hashedFingerprint.HashBins, hashedFingerprint.SequenceNumber) },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = ThresholdVotes,
                            Clusters = new[] { "first-group-id" }
                        }).Matches.Select(info => info.SubFingerprint).ToList();

                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(firstTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(
                    new[] { new QueryHash(hashedFingerprint.HashBins, hashedFingerprint.SequenceNumber) },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = ThresholdVotes,
                            Clusters = new[] { "second-group-id" }
                        }).Matches.Select(info => info.SubFingerprint).ToList();


                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(secondTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(
                        new[] { new QueryHash(hashedFingerprint.HashBins, hashedFingerprint.SequenceNumber) },
                        new DefaultQueryConfiguration { ThresholdVotes = ThresholdVotes }).Matches.Select(info => info.SubFingerprint).ToList();

                Assert.AreEqual(2, subFingerprintData.Count);
            }
        }

        [Test]
        public void ReadHashDataByTrackTest()
        {
            var firstTrack = new TrackInfo("isrc", "artist", "title", 200);

            var firstTrackReference = trackDao.InsertTrack(firstTrack).TrackReference;

            var firstHashData = FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash()
                .Result;

            InsertHashedFingerprintsForTrack(firstHashData, firstTrackReference);

            var secondTrack = new TrackInfo("isrc", "artist", "title", 200);

            var secondTrackReference = trackDao.InsertTrack(secondTrack).TrackReference;

            var secondHashData = FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash()
                .Result;

            InsertHashedFingerprintsForTrack(secondHashData, secondTrackReference);

            var resultFirstHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(firstTrackReference);
            AssertHashDatasAreTheSame(firstHashData, resultFirstHashData);

            var resultSecondHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(secondTrackReference);
            AssertHashDatasAreTheSame(secondHashData, resultSecondHashData);
        }

        private void InsertHashedFingerprintsForTrack(IEnumerable<HashedFingerprint> hashedFingerprints, IModelReference trackReference)
        {
            subFingerprintDao.InsertHashDataForTrack(hashedFingerprints, trackReference);
        }
    }
}
