namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class HashBinDaoTest : AbstractIntegrationTest
    {
        private readonly HashBinDao hashBinDao;
        
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly ITagService tagService;

        private readonly TrackDao trackDao;
        private readonly SubFingerprintDao subFingerprintDao;

        public HashBinDaoTest()
        {
            hashBinDao = new HashBinDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());

            fingerprintCommandBuilder = DependencyResolver.Current.Get<IFingerprintCommandBuilder>();
            tagService = DependencyResolver.Current.Get<ITagService>();
            
            trackDao = new TrackDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
            subFingerprintDao = new SubFingerprintDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
        }

        [TestMethod]
        public void InsertReadTest()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            int trackId = trackDao.Insert(track);
            long subFingerprintId = subFingerprintDao.Insert(GenericSignature, trackId);

            hashBinDao.Insert(GenericHashBuckets, subFingerprintId);

            for (int hashTable = 1; hashTable <= GenericHashBuckets.Length; hashTable++)
            {
                var hashBins = hashBinDao.ReadHashBinsByHashTable(hashTable);
                Assert.AreEqual(1, hashBins.Count);
                Assert.AreEqual(GenericHashBuckets[hashTable - 1], hashBins[0].HashBin);
            }
        }

        [TestMethod]
        public void SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSong()
        {
            const int StaticStride = 5115;
            TagInfo tagInfo = tagService.GetTagInfo(PathToMp3);
            int releaseYear = tagInfo.Year;
            TrackData track = new TrackData(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            int trackId = trackDao.Insert(track);
            var hashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3)
                .WithFingerprintConfig(config =>
                {
                    config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .Hash()
                .Result;

            foreach (var hash in hashData)
            {
                long subFingerprintId = subFingerprintDao.Insert(hash.SubFingerprint, trackId);
                hashBinDao.Insert(hash.HashBins, subFingerprintId);
            }

            for (int hashTable = 1; hashTable <= 25; hashTable++)
            {
                var hashBins = hashBinDao.ReadHashBinsByHashTable(hashTable);
                Assert.AreEqual(hashData.Count, hashBins.Count);
            }
        }
    }
}
