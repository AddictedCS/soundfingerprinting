namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;

    [TestClass]
    public abstract class AbstractSubFingerprintDaoTest : AbstractIntegrationTest
    {
        public abstract ISubFingerprintDao SubFingerprintDao { get; set; }

        public abstract ITrackDao TrackDao { get; set; }

        [TestMethod]
        public void InsertTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = TrackDao.InsertTrack(track);
            
            var subFingerprintReference = SubFingerprintDao.InsertSubFingerprint(GenericSignature, trackReference);

            AssertModelReferenceIsInitialized(subFingerprintReference);
        }

        [TestMethod]
        public void ReadTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = TrackDao.InsertTrack(track);
            var subFingerprintReference = SubFingerprintDao.InsertSubFingerprint(GenericSignature, trackReference);

            SubFingerprintData actual = SubFingerprintDao.ReadSubFingerprint(subFingerprintReference);

            AsserSubFingerprintsAreEqual(new SubFingerprintData(GenericSignature, subFingerprintReference, trackReference), actual);
        }

        private void AsserSubFingerprintsAreEqual(SubFingerprintData expected, SubFingerprintData actual)
        {
            Assert.AreEqual(expected.SubFingerprintReference, actual.SubFingerprintReference);
            Assert.AreEqual(expected.TrackReference, actual.TrackReference);
            for (int i = 0; i < expected.Signature.Length; i++)
            {
                Assert.AreEqual(expected.Signature[i], actual.Signature[i]);
            }
        }
    }
}
