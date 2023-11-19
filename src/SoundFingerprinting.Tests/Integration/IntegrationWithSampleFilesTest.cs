namespace SoundFingerprinting.Tests.Integration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    using Audio;
    using Data;
    using ProtoBuf;

    using NUnit.Framework;

    public abstract class IntegrationWithSampleFilesTest : AbstractTest
    {
        protected readonly string pathToSamples = Path.Combine(TestContext.CurrentContext.TestDirectory, "chopinsamples.bin");
        
        protected readonly string PathToWav = Path.Combine(TestContext.CurrentContext.TestDirectory, "chopin_short.wav");

        protected void AssertHashDatasAreTheSame(Hashes h1, Hashes h2)
        {
            var firstHashes = h1.ToList();
            var secondHashes = h2.ToList();
            Assert.AreEqual(h1.DurationInSeconds, h2.DurationInSeconds);
            Assert.AreEqual(firstHashes.Count, secondHashes.Count);
         
            // hashes are not ordered as parallel computation is involved
            firstHashes = SortHashesBySequenceNumber(firstHashes);
            secondHashes = SortHashesBySequenceNumber(secondHashes);

            for (int i = 0; i < firstHashes.Count; i++)
            {
                Assert.AreEqual(firstHashes[i].SequenceNumber, secondHashes[i].SequenceNumber);
                Assert.AreEqual(firstHashes[i].StartsAt, secondHashes[i].StartsAt, 0.0001);
                CollectionAssert.AreEqual(firstHashes[i].HashBins, secondHashes[i].HashBins);
            }
        }

        protected TagInfo GetTagInfo()
        {
            return new TagInfo
            {
                Album = "Album",
                AlbumArtist = "AlbumArtist",
                Artist = "Chopin",
                Composer = "Composer",
                Duration = 10.0d,
                Genre = "Genre",
                IsEmpty = false,
                ISRC = "ISRC",
                Title = "Nocture",
                Year = 1857
            };
        }

        protected AudioSamples GetAudioSamples()
        {
            lock (this)
            {
                using Stream stream = new FileStream(pathToSamples, FileMode.Open, FileAccess.Read);
                return Serializer.DeserializeWithLengthPrefix<AudioSamples>(stream, PrefixStyle.Fixed32);
            }
        }

        private static List<HashedFingerprint> SortHashesBySequenceNumber(IEnumerable<HashedFingerprint> hashDatasFromFile)
        {
            return hashDatasFromFile.OrderBy(hashData => hashData.SequenceNumber).ToList();
        }
    }
}
