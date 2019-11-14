namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
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
            var ramStorage = new RAMStorage(25);
            subFingerprintDao = new SubFingerprintDao(ramStorage, new StandardGroupingCounter());
            trackDao = new TrackDao(ramStorage);
        }

        [Test]
        public void ShouldInsertAndReadSubFingerprints()
        {
            var track = new TrackInfo("id", "title", "artist");
            const int numberOfHashBins = 100;
            var trackReference = trackDao.InsertTrack(track, 200).TrackReference;
            var genericHashBuckets = GenericHashBuckets();
            var hashedFingerprints = Enumerable.Range(0, numberOfHashBins).Select(
                sequenceNumber => new HashedFingerprint(
                    genericHashBuckets,
                    (uint)sequenceNumber,
                    sequenceNumber * 0.928f,
                    Enumerable.Empty<string>()));

            InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var fingerprints = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference)
                                                .Select(ToHashedFingerprint())
                                                .ToList();

            Assert.AreEqual(numberOfHashBins, fingerprints.Count);
            foreach (var hashedFingerprint in fingerprints)
            {
                CollectionAssert.AreEqual(genericHashBuckets, hashedFingerprint.HashBins);
            }
        }

        [Test]
        public async Task SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSongTest()
        {
            var track = new TrackInfo("id", "title", "artist");
            var trackReference = trackDao.InsertTrack(track, 120).TrackReference;
            var hashedFingerprints = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var hashes = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference)
                                          .Select(ToHashedFingerprint())
                                          .ToList();

            Assert.AreEqual(hashedFingerprints.Count(), hashes.Count);
            foreach (var data in hashes)
            {
                Assert.AreEqual(25, data.HashBins.Length);
            }
        }

        [Test]
        public async Task ReadByTrackGroupIdWorksAsExpectedTest()
        {
            var firstTrack = new TrackInfo("id", "title", "artist");
            var secondTrack = new TrackInfo("id", "title", "artist");

            var firstTrackReference = trackDao.InsertTrack(firstTrack, 120).TrackReference;
            var secondTrackReference = trackDao.InsertTrack(secondTrack, 120).TrackReference;

            var hashedFingerprintsForFirstTrack = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .WithFingerprintConfig(config =>
                {
                    config.Clusters = new[] { "first-group-id" };
                    return config;
                })
                .UsingServices(audioService)
                .Hash();

            InsertHashedFingerprintsForTrack(hashedFingerprintsForFirstTrack, firstTrackReference);

            var hashedFingerprintsForSecondTrack = await FingerprintCommandBuilder.Instance
               .BuildFingerprintCommand()
               .From(GetAudioSamples())
               .WithFingerprintConfig(config =>
               {
                   config.Clusters = new[] { "second-group-id" };
                   return config;
               })
               .UsingServices(audioService)
               .Hash();

            InsertHashedFingerprintsForTrack(hashedFingerprintsForSecondTrack, secondTrackReference);

            const int thresholdVotes = 25;

            foreach (var hashedFingerprint in hashedFingerprintsForFirstTrack)
            {
                var subFingerprintData = subFingerprintDao.ReadSubFingerprints(
                    new[] { hashedFingerprint.HashBins },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = thresholdVotes,
                            Clusters = new[] { "first-group-id" }
                        }).ToList();

                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(firstTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(
                    new[] { hashedFingerprint.HashBins },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = thresholdVotes,
                            Clusters = new[] { "second-group-id" }
                        }).ToList();

                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(secondTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(new[] { hashedFingerprint.HashBins }, new DefaultQueryConfiguration { ThresholdVotes = thresholdVotes })
                        .ToList();

                Assert.AreEqual(2, subFingerprintData.Count);
            }
        }

        [Test]
        public async Task ReadHashDataByTrackTest()
        {

            var firstHashData = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var firstTrack = new TrackInfo("id", "title", "artist");
            var firstTrackReference = trackDao.InsertTrack(firstTrack, firstHashData.DurationInSeconds).TrackReference;
            
            InsertHashedFingerprintsForTrack(firstHashData, firstTrackReference);

            var secondHashData = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            var secondTrack = new TrackInfo("id", "title", "artist");
            var secondTrackReference = trackDao.InsertTrack(secondTrack, secondHashData.DurationInSeconds).TrackReference;
            InsertHashedFingerprintsForTrack(secondHashData, secondTrackReference);

            var resultFirstHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(firstTrackReference)
                                                       .Select(ToHashedFingerprint())
                                                       .ToList();

            AssertHashDatasAreTheSame(firstHashData, new Hashes(resultFirstHashData, firstHashData.DurationInSeconds));

            var resultSecondHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(secondTrackReference)
                                                        .Select(ToHashedFingerprint())
                                                        .ToList();

            AssertHashDatasAreTheSame(secondHashData, new Hashes(resultSecondHashData, secondHashData.DurationInSeconds));
        }

        private static Func<SubFingerprintData, HashedFingerprint> ToHashedFingerprint()
        {
            return subFingerprint => new HashedFingerprint(
                subFingerprint.Hashes,
                subFingerprint.SequenceNumber,
                subFingerprint.SequenceAt,
                subFingerprint.Clusters);
        }

        private void InsertHashedFingerprintsForTrack(IEnumerable<HashedFingerprint> hashedFingerprints, IModelReference trackReference)
        {
            subFingerprintDao.InsertHashDataForTrack(hashedFingerprints, trackReference);
        }
    }
}