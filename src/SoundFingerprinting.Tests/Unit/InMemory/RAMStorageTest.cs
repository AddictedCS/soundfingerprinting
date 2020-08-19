namespace SoundFingerprinting.Tests.Unit.InMemory
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
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
            float one = 8192f / 5512;
            Parallel.For(0, tracksCount, i =>
            {
                var trackReference = new ModelReference<uint>((uint)i);
                for (int j = 0; j < subFingerprintsPerTrack; ++j)
                {
                    var subFingerprintData = new SubFingerprintData(longs, (uint)j, j * one, new ModelReference<uint>((uint)j), trackReference, Array.Empty<byte>());
                    storage.AddSubFingerprint(subFingerprintData);
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
