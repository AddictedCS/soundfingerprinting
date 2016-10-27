namespace SoundFingerprinting.Tests.Integration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.InMemory;

    [TestClass]
    public class SubFingerprintDaoTest : IntegrationWithSampleFilesTest
    {
        private ISubFingerprintDao subFingerprintDao;
        private ITrackDao trackDao;

        [TestInitialize]
        public void SetUp()
        {
            var ramStorage = new RAMStorage(NumberOfHashTables);
            subFingerprintDao = new SubFingerprintDao(ramStorage);
            trackDao = new TrackDao(ramStorage);
        }

        [TestMethod]
        public void InsertTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = trackDao.InsertTrack(track);
            
            var subFingerprintReference = subFingerprintDao.InsertSubFingerprint(GenericSignature, 123, 0.928, trackReference);

            AssertModelReferenceIsInitialized(subFingerprintReference);
        }

        [TestMethod]
        public void ReadTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = trackDao.InsertTrack(track);
            var subFingerprintReference = subFingerprintDao.InsertSubFingerprint(GenericSignature, 123, 0.928, trackReference);

            SubFingerprintData actual = subFingerprintDao.ReadSubFingerprint(subFingerprintReference);

            AsserSubFingerprintsAreEqual(new SubFingerprintData(GenericSignature, 123, 0.928, subFingerprintReference, trackReference), actual);
        }

        private void AsserSubFingerprintsAreEqual(SubFingerprintData expected, SubFingerprintData actual)
        {
            Assert.AreEqual(expected.SubFingerprintReference, actual.SubFingerprintReference);
            Assert.AreEqual(expected.TrackReference, actual.TrackReference);
            for (int i = 0; i < expected.Signature.Length; i++)
            {
                Assert.AreEqual(expected.Signature[i], actual.Signature[i]);
            }

            Assert.AreEqual(expected.SequenceNumber, actual.SequenceNumber);
            Assert.IsTrue(System.Math.Abs(expected.SequenceAt - actual.SequenceAt) < Epsilon);
        }
    }
}
