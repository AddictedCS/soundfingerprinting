namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;

    public abstract class AbstractSubFingerprintDaoTest : AbstractIntegrationTest
    {
        public abstract ISubFingerprintDao SubFingerprintDao { get; set; }

        public abstract ITrackDao TrackDao { get; set; }

        [TestMethod]
        public void InsertTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = TrackDao.Insert(track);
            
            long subFingerprintId = SubFingerprintDao.Insert(GenericSignature, trackId);

            Assert.IsFalse(subFingerprintId == 0);
        }

        [TestMethod]
        public void ReadTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = TrackDao.Insert(track);
            
            long subFingerprintId = SubFingerprintDao.Insert(GenericSignature, trackId);

            SubFingerprintData actual = SubFingerprintDao.ReadById(subFingerprintId);

            AsserSubFingerprintsAreEqual(
                new SubFingerprintData(
                    GenericSignature,
                    new ModelReference<long>(subFingerprintId),
                    new ModelReference<int>(trackId)),
                actual);
        }

        private void AsserSubFingerprintsAreEqual(SubFingerprintData expected, SubFingerprintData actual)
        {
            Assert.AreEqual(expected.SubFingerprintReference.HashCode, actual.SubFingerprintReference.HashCode);
            Assert.AreEqual(expected.TrackReference.HashCode, actual.TrackReference.HashCode);
            for (int i = 0; i < expected.Signature.Length; i++)
            {
                Assert.AreEqual(expected.Signature[i], actual.Signature[i]);
            }
        }
    }
}
