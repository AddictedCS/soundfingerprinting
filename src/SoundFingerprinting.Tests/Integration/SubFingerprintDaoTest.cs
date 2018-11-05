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
            var hashedFingerprints = Enumerable.Range(0, NumberOfHashBins).Select(
                sequenceNumber => new HashedFingerprint(
                    genericHashBuckets,
                    (uint)sequenceNumber,
                    sequenceNumber * 0.928f,
                    Enumerable.Empty<string>()));

            InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var fingerprints = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference)
                                                .Select(ToHashedFingerprint())
                                                .ToList();

            Assert.AreEqual(NumberOfHashBins, fingerprints.Count);
            foreach (var hashedFingerprint in fingerprints)
            {
                CollectionAssert.AreEqual(genericHashBuckets, hashedFingerprint.HashBins);
            }
        }

        [Test]
        public async Task SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSongTest()
        {
            var track = new TrackInfo("isrc", "artist", "title", 120d);
            var trackReference = trackDao.InsertTrack(track).TrackReference;
            var hashedFingerprints = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);

            var hashes = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference)
                                          .Select(ToHashedFingerprint())
                                          .ToList();
            
            Assert.AreEqual(hashedFingerprints.Count, hashes.Count);
            foreach (var data in hashes)
            {
                Assert.AreEqual(25, data.HashBins.Length);
            }
        }

        [Test]
        public async Task ReadByTrackGroupIdWorksAsExpectedTest()
        {
            var firstTrack = new TrackInfo("isrc", "artist", "title", 120d);
            var secondTrack = new TrackInfo("isrc", "artist", "title", 120d);

            var firstTrackReference = trackDao.InsertTrack(firstTrack).TrackReference;
            var secondTrackReference = trackDao.InsertTrack(secondTrack).TrackReference;

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

            const int ThresholdVotes = 25;

            foreach (var hashedFingerprint in hashedFingerprintsForFirstTrack)
            {
                var subFingerprintData = subFingerprintDao.ReadSubFingerprints(
                    new[] { hashedFingerprint.HashBins },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = ThresholdVotes,
                            Clusters = new[] { "first-group-id" }
                        }).ToList();

                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(firstTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(
                    new[] { hashedFingerprint.HashBins },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = ThresholdVotes,
                            Clusters = new[] { "second-group-id" }
                        }).ToList();


                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(secondTrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(new[] { hashedFingerprint.HashBins }, new DefaultQueryConfiguration { ThresholdVotes = ThresholdVotes })
                        .ToList();

                Assert.AreEqual(2, subFingerprintData.Count);
            }
        }

        [Test]
        public async Task ReadHashDataByTrackTest()
        {
            var firstTrack = new TrackInfo("isrc", "artist", "title", 200);

            var firstTrackReference = trackDao.InsertTrack(firstTrack).TrackReference;

            var firstHashData = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            InsertHashedFingerprintsForTrack(firstHashData, firstTrackReference);

            var secondTrack = new TrackInfo("isrc", "artist", "title", 200);

            var secondTrackReference = trackDao.InsertTrack(secondTrack).TrackReference;

            var secondHashData = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .UsingServices(audioService)
                .Hash();

            InsertHashedFingerprintsForTrack(secondHashData, secondTrackReference);

            var resultFirstHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(firstTrackReference)
                                                       .Select(ToHashedFingerprint())
                                                       .ToList();
            
            AssertHashDatasAreTheSame(firstHashData, resultFirstHashData);

            var resultSecondHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(secondTrackReference)
                                                        .Select(ToHashedFingerprint())
                                                        .ToList();
            
            AssertHashDatasAreTheSame(secondHashData, resultSecondHashData);
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
