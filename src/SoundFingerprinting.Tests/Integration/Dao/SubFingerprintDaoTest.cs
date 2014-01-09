namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Infrastructure;

    public class SubFingerprintDaoTest : AbstractIntegrationTest
    {
        private readonly SubFingerprintDao subFingerprintDao;
        private readonly TrackDao trackDao;

        public SubFingerprintDaoTest()
        {
            subFingerprintDao = new SubFingerprintDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
            trackDao = new TrackDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
        }

        [TestMethod]
        public void InsertTest()
        {
            Track track = new Track("isrc", "artist", "title", "album", 1986, 200);
            trackDao.Insert(track);
            SubFingerprint subFingerprint = new SubFingerprint(GenericSignature, track.Id);

            subFingerprintDao.Insert(subFingerprint);

            Assert.IsFalse(subFingerprint.Id == 0);
        }

        [TestMethod]
        public void ReadTest()
        {
            Track track = new Track("isrc", "artist", "title", "album", 1986, 200);
            trackDao.Insert(track);
            SubFingerprint expected = new SubFingerprint(GenericSignature, track.Id);
            subFingerprintDao.Insert(expected);

            SubFingerprint actual = subFingerprintDao.ReadById(expected.Id);

            AsserSubFingerprintsAreEqual(expected, actual);
        }

        private void AsserSubFingerprintsAreEqual(SubFingerprint expected, SubFingerprint actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.TrackId, actual.TrackId);
            for (int i = 0; i < expected.Signature.Length; i++)
            {
                Assert.AreEqual(expected.Signature[i], actual.Signature[i]);
            }
        }
    }
}
