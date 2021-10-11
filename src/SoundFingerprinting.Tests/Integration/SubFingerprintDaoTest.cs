#nullable enable
namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
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
            var track = new TrackInfo("id", string.Empty, string.Empty);
            const int numberOfHashBins = 100;
            var genericHashBuckets = GenericHashBuckets();
            var hashedFingerprints = Enumerable
                .Range(0, numberOfHashBins)
                .Select(sequenceNumber => new HashedFingerprint(
                    genericHashBuckets,
                    (uint)sequenceNumber,
                    sequenceNumber * 0.928f, 
                    Array.Empty<byte>()));

            var trackData = InsertTrackAndHashes(track, new Hashes(hashedFingerprints, (numberOfHashBins + 1) * 0.928f, MediaType.Audio));
            var fingerprints = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackData.TrackReference)
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
            var track = new TrackInfo("id", string.Empty, string.Empty);
            var hashedFingerprints = await GetHashedFingerprints();
            var trackData = InsertTrackAndHashes(track, hashedFingerprints);

            var hashes = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackData.TrackReference)
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
            var modelReferenceTracker = new UIntModelReferenceTracker();
            var firstTrack = new TrackInfo("id-1",string.Empty, string.Empty,new Dictionary<string, string>{{ "group-id", "first-group-id"}}, MediaType.Audio);
            var hashedFingerprintsForFirstTrack = await GetHashedFingerprints();
            var firstTrackData = InsertTrackAndHashes(firstTrack, hashedFingerprintsForFirstTrack, modelReferenceTracker);
            
            var secondTrack = new TrackInfo("id-2",string.Empty, string.Empty, new Dictionary<string, string>{{ "group-id",  "second-group-id"}}, MediaType.Audio);
            var hashedFingerprintsForSecondTrack = await GetHashedFingerprints();
            var secondTrackData = InsertTrackAndHashes(secondTrack, hashedFingerprintsForSecondTrack, modelReferenceTracker);
            
            const int thresholdVotes = 25;
            var queryConfigWithFirstGroupId = new DefaultQueryConfiguration
            {
                ThresholdVotes = thresholdVotes,
                YesMetaFieldsFilters = new Dictionary<string, string>{{ "group-id", "first-group-id" }}
            };
            
            var queryConfigWithSecondGroupId = new DefaultQueryConfiguration
            {
                ThresholdVotes = thresholdVotes,
                YesMetaFieldsFilters = new Dictionary<string, string>{{ "group-id", "second-group-id" }}
            };
            
            foreach (var hashedFingerprint in hashedFingerprintsForFirstTrack)
            {
                var subFingerprintData = subFingerprintDao.ReadSubFingerprints(new[] { hashedFingerprint.HashBins }, queryConfigWithFirstGroupId).ToList();
                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(firstTrackData.TrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(new[] { hashedFingerprint.HashBins }, queryConfigWithSecondGroupId).ToList();
                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(secondTrackData.TrackReference, subFingerprintData[0].TrackReference);

                subFingerprintData = subFingerprintDao
                        .ReadSubFingerprints(new[] { hashedFingerprint.HashBins }, new DefaultQueryConfiguration { ThresholdVotes = thresholdVotes })
                        .ToList();

                Assert.AreEqual(2, subFingerprintData.Count);
            }
        }

        [Test]
        public async Task ReadHashDataByTrackTest()
        {
            var modelReferenceTracker = new UIntModelReferenceTracker();
            var firstHashData = await GetHashedFingerprints();
            var firstTrack = InsertTrackAndHashes(new TrackInfo("id-1", string.Empty, string.Empty), firstHashData, modelReferenceTracker);

            var secondHashData = await GetHashedFingerprints();
            var secondTrack = InsertTrackAndHashes(new TrackInfo("id-2", string.Empty, string.Empty), secondHashData);

            var resultFirstHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(firstTrack.TrackReference)
                                                       .Select(ToHashedFingerprint())
                                                       .ToList();

            AssertHashDatasAreTheSame(firstHashData, new Hashes(resultFirstHashData, firstHashData.DurationInSeconds, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));

            var resultSecondHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(secondTrack.TrackReference)
                                                        .Select(ToHashedFingerprint())
                                                        .ToList();

            AssertHashDatasAreTheSame(secondHashData, new Hashes(resultSecondHashData, secondHashData.DurationInSeconds, MediaType.Audio, DateTime.Now, Enumerable.Empty<string>()));
        }

        private static Func<SubFingerprintData, HashedFingerprint> ToHashedFingerprint()
        {
            return _ => new HashedFingerprint(_.Hashes, _.SequenceNumber, _.SequenceAt, _.OriginalPoint);
        }
        
        private async Task<Hashes> GetHashedFingerprints()
        {
            return await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .Hash();
        }
        
        private TrackData InsertTrackAndHashes(TrackInfo track, Hashes hashedFingerprints, UIntModelReferenceTracker? modelReferenceTracker = null)
        {
            modelReferenceTracker ??= new UIntModelReferenceTracker();
            var (trackData, subFingerprints) = modelReferenceTracker.AssignModelReferences(track, hashedFingerprints);
            trackDao.InsertTrack(trackData);
            subFingerprintDao.InsertSubFingerprints(subFingerprints);
            return trackData;
        }
    }
}