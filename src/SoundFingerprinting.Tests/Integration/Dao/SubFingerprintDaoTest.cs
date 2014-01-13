namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    [TestClass]
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
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = trackDao.Insert(track);
            
            long subFingerprintId = subFingerprintDao.Insert(GenericSignature, trackId);

            Assert.IsFalse(subFingerprintId == 0);
        }

        [TestMethod]
        public void ReadTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = trackDao.Insert(track);
            
            long subFingerprintId = subFingerprintDao.Insert(GenericSignature, trackId);

            SubFingerprintData actual = subFingerprintDao.ReadById(subFingerprintId);

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
