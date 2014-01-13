namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.SQL;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    [TestClass]
    public class FingerprintDaoTest : AbstractIntegrationTest
    {
        private readonly FingerprintDao fingerprintDao;
        private readonly TrackDao trackDao;

        public FingerprintDaoTest()
        {
            fingerprintDao = new FingerprintDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
            trackDao = new TrackDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
        }

        [TestMethod]
        public void InsertTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = trackDao.Insert(track);

            int id = fingerprintDao.Insert(GenericFingerprint, trackId);

            Assert.IsFalse(id == 0);
        }

        [TestMethod]
        public void ReadTest()
        {
            const int NumberOfFingerprints = 100;
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = trackDao.Insert(track);

            for (int i = 0; i < NumberOfFingerprints; i++)
            {
                fingerprintDao.Insert(GenericFingerprint, trackId);
            }
           
            var fingerprints = fingerprintDao.ReadFingerprintsByTrackId(trackId);

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
