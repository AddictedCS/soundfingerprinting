namespace SoundFingerprinting.Tests.Integration
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.InMemory;

    [TestFixture]
    public class FingerprintDaoTest : AbstractTest
    {
        private IFingerprintDao fingerprintDao;
        private ITrackDao trackDao;

        [SetUp]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            fingerprintDao = new FingerprintDao(ramStorage);
            trackDao = new TrackDao(ramStorage);
        }

        [Test]
        public void ShouldInsertFingerprintInStorage()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = trackDao.InsertTrack(track);

            var fingerprintReference = fingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint(), trackReference));

            AssertModelReferenceIsInitialized(fingerprintReference);
        }

        [Test]
        public void ShouldInsertMultipleFingerprintsInStorage()
        {
            const int NumberOfFingerprints = 100;
            var trackReference = InsertTrack();

            for (int i = 0; i < NumberOfFingerprints; i++)
            {
                var fingerprintReference = fingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint(), trackReference));
                AssertModelReferenceIsInitialized(fingerprintReference);
            }
        }
        
        [Test]
        public void ShouldReadFingerprintsFromStorage()
        {
            const int NumberOfFingerprints = 100;
            var trackReference = InsertTrack();

            for (int i = 0; i < NumberOfFingerprints; i++)
            {
                fingerprintDao.InsertFingerprint(new FingerprintData(GenericFingerprint(), trackReference));
            }

            var fingerprints = fingerprintDao.ReadFingerprintsByTrackReference(trackReference);

            Assert.AreEqual(NumberOfFingerprints, fingerprints.Count);
            foreach (var fingerprint in fingerprints)
            {
                CollectionAssert.AreEqual(GenericFingerprint(), fingerprint.Signature);
            }
        }

        private IModelReference InsertTrack()
        {
            var trackData = new TrackData("isrc", "artist", "title", "album", 2012, 200);
            return trackDao.InsertTrack(trackData);
        }
    }
}
