namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Data;

    [TestClass]
    public abstract class AbstractFingerprintDaoTest : AbstractIntegrationTest
    {
        public abstract IFingerprintDao FingerprintDao { get; set; }

        public abstract ITrackDao TrackDao { get; set; }

        [TestMethod]
        public void InsertTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = TrackDao.Insert(track);

            int id = FingerprintDao.Insert(GenericFingerprint, trackId);

            Assert.IsFalse(id == 0);
        }

        [TestMethod]
        public void MultipleFingerprintsInsertTest()
        {
            for (int i = 0; i < 100; i++)
            {
                var trackData = new TrackData("isrc" + i, "artist", "title", "album", 2012, 200);
                int trackId = TrackDao.Insert(trackData);
                int id = FingerprintDao.Insert(GenericFingerprint, trackId);
                Assert.IsFalse(id == 0);
            }
        }

        [TestMethod]
        public void ReadTest()
        {
            const int NumberOfFingerprints = 100;
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = TrackDao.Insert(track);

            for (int i = 0; i < NumberOfFingerprints; i++)
            {
                FingerprintDao.Insert(GenericFingerprint, trackId);
            }

            var fingerprints = FingerprintDao.ReadFingerprintsByTrackId(trackId);

            Assert.IsTrue(fingerprints.Count == NumberOfFingerprints);

            foreach (var fingerprint in fingerprints)
            {
                Assert.IsTrue(GenericFingerprint.Length == fingerprint.Signature.Length);
                for (var i = 0; i < GenericFingerprint.Length; i++)
                {
                    Assert.AreEqual(GenericFingerprint[i], fingerprint.Signature[i]);
                }
            }
        }
    }
}
