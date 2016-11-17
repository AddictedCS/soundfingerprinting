namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;

    [DeploymentItem(@"TestEnvironment\floatsamples.bin")]
    [DeploymentItem(@"TestEnvironment\Kryptonite.mp3")]
    [TestClass]
    public abstract class IntegrationWithSampleFilesTest : AbstractTest
    {
        protected const int NumberOfHashTables = 25;

        protected readonly IFingerprintCommandBuilder FingerprintCommandBuilder = new FingerprintCommandBuilder();
        protected readonly IAudioService AudioService = new NAudioService();

        protected void AssertHashDatasAreTheSame(IList<HashedFingerprint> firstHashDatas, IList<HashedFingerprint> secondHashDatas)
        {
            Assert.AreEqual(firstHashDatas.Count, secondHashDatas.Count);
         
            // hashes are not ordered as parallel computation is involved
            firstHashDatas = SortHashesByFirstValueOfHashBin(firstHashDatas);
            secondHashDatas = SortHashesByFirstValueOfHashBin(secondHashDatas);

            for (int i = 0; i < firstHashDatas.Count; i++)
            {
                CollectionAssert.AreEqual(firstHashDatas[i].SubFingerprint, secondHashDatas[i].SubFingerprint);
                CollectionAssert.AreEqual(firstHashDatas[i].HashBins, secondHashDatas[i].HashBins);
                Assert.AreEqual(firstHashDatas[i].SequenceNumber, secondHashDatas[i].SequenceNumber);
                Assert.AreEqual(firstHashDatas[i].Timestamp, secondHashDatas[i].Timestamp, Epsilon);
            }
        }

        protected void AssertModelReferenceIsInitialized(IModelReference modelReference)
        {
            Assert.IsNotNull(modelReference);
            Assert.IsTrue(modelReference.GetHashCode() != 0);
        }

        protected TagInfo GetTagInfo()
        {
            return new TagInfo
            {
                Album = "Album",
                AlbumArtist = "AlbumArtist",
                Artist = "Artist",
                Composer = "Composer",
                Duration = 100.2,
                Genre = "Genre",
                IsEmpty = false,
                ISRC = "ISRC",
                Title = "Title",
                Year = 1986
            };
        }

        private List<HashedFingerprint> SortHashesByFirstValueOfHashBin(IEnumerable<HashedFingerprint> hashDatasFromFile)
        {
            return hashDatasFromFile.OrderBy(hashData => hashData.SequenceNumber).ToList();
        }
    }
}
