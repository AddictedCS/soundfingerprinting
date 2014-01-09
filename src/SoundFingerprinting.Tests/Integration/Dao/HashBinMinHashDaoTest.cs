namespace SoundFingerprinting.Tests.Integration.Dao
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    public class HashBinMinHashDaoTest : AbstractIntegrationTest
    {
        private readonly HashBinMinHashDao hashBinMinHashDao;
        
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly ITagService tagService;

        private readonly TrackDao trackDao;
        private readonly SubFingerprintDao subFingerprintDao;

        public HashBinMinHashDaoTest()
        {
            hashBinMinHashDao = new HashBinMinHashDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());

            fingerprintCommandBuilder = DependencyResolver.Current.Get<IFingerprintCommandBuilder>();
            tagService = DependencyResolver.Current.Get<ITagService>();
            
            trackDao = new TrackDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
            subFingerprintDao = new SubFingerprintDao(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>());
        }

        [TestMethod]
        public void InsertReadTest()
        {
            Track track = new Track("isrc", "artist", "title", "album", 1986, 200);
            trackDao.Insert(track);
            SubFingerprint subFingerprint = new SubFingerprint(GenericSignature, track.Id);
            subFingerprintDao.Insert(subFingerprint);

            int hashTable = 1;
            foreach (var b in GenericSignature)
            {
                HashBinMinHash hashBinMinHash = new HashBinMinHash(b, hashTable++, subFingerprint.Id);
                hashBinMinHashDao.Insert(hashBinMinHash);
                var hashBins = hashBinMinHashDao.ReadHashBinsByHashTable(hashTable);
                Assert.AreEqual(1, hashBins.Count);
                Assert.AreEqual(b, hashBins[0]);
            }
        }

        [TestMethod]
        public void SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSong()
        {
            const int StaticStride = 5115;
            TagInfo tagInfo = tagService.GetTagInfo(PathToMp3);
            int releaseYear = tagInfo.Year;
            Track track = new Track(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            trackDao.Insert(track);
            var hashData = fingerprintCommandBuilder
                .BuildFingerprintCommand()
                .From(PathToMp3)
                .WithCustomAlgorithmConfiguration(config =>
                {
                    config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                })
                .Hash()
                .Result;

            foreach (var hash in hashData)
            {
                var subFingerprint = new SubFingerprint(hash.SubFingerprint, track.Id);
                subFingerprintDao.Insert(subFingerprint);
                int tableNumber = 1;
                foreach (var hashBin in hash.HashBins)
                {
                    var hashBinModel = new HashBinMinHash(hashBin, tableNumber++, subFingerprint.Id);
                    hashBinMinHashDao.Insert(hashBinModel);
                }
            }

            for (int hashTable = 1; hashTable <= 25; hashTable++)
            {
                var hashBins = hashBinMinHashDao.ReadHashBinsByHashTable(hashTable);
                Assert.AreEqual(hashData.Count, hashBins.Count);
            }
        }
    }
}
