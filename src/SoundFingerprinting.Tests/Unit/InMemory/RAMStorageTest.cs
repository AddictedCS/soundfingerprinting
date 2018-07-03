namespace SoundFingerprinting.Tests.Unit.InMemory
{
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Math;

    [TestFixture]
    public class RAMStorageTest
    {
        [Test]
        public void ShouldInsertEntriesInThreadSafeManner()
        {
            var storage = new RAMStorage(50);
            var hashConverter = new HashConverter();

            var hashes = Enumerable.Range(0, 100).Select(b => (byte)b).ToArray();
            var longs = hashConverter.ToInts(hashes, 25);

            int tracksCount = 520;
            int subFingerprintsPerTrack = 33;
            Parallel.For(0, tracksCount, i =>
            {
                var trackReference = new ModelReference<int>(i);
                for (int j = 0; j < subFingerprintsPerTrack; ++j)
                {
                    var hashed = new HashedFingerprint(longs, (uint)j, j * 1.48f, Enumerable.Empty<string>());
                    storage.AddSubfingerprint(hashed, trackReference);
                }
            });

            for (int i = 0; i < 25; ++i)
            {
                var subFingerprints = storage.GetSubFingerprintsByHashTableAndHash(i, longs[i]);
                Assert.AreEqual(tracksCount * subFingerprintsPerTrack, subFingerprints.Count);
            }
        }
    }
}
