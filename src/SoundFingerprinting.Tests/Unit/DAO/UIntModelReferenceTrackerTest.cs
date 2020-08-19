namespace SoundFingerprinting.Tests.Unit.DAO
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    [TestFixture]
    public class UIntModelReferenceTrackerTest : AbstractTest
    {
        [Test]
        public void ShouldCopyAllFieldsCorrectly()
        {
            var modelReferenceTracker = new UIntModelReferenceTracker(0, 0);
            var track = new TrackInfo("id", "title", "artist", new Dictionary<string, string>(){{"key", "value"}}, MediaType.Audio | MediaType.Video);
            var hashes = GetHashes(1);

            var (trackData, subFingerprints) = modelReferenceTracker.AssignModelReferences(track, hashes);
            
            AssertTracksAreEqual(track, trackData);
            var enumerable = subFingerprints as SubFingerprintData[] ?? subFingerprints.ToArray();
            Assert.AreEqual(1, enumerable.Count());

            var hash = hashes.First();
            var subFingerprintData = enumerable.First();
            CollectionAssert.AreEqual(hash.HashBins, subFingerprintData.Hashes);
            CollectionAssert.AreEqual(hash.OriginalPoint, subFingerprintData.OriginalPoint);
            Assert.AreEqual(hash.SequenceNumber, subFingerprintData.SequenceNumber);
            Assert.AreEqual(hash.StartsAt, subFingerprintData.SequenceAt);
        }
        
        [Test]
        public void ShouldResetModelReferences()
        {
            var modelReferenceTracker = new UIntModelReferenceTracker(0, 0);
            var track = new TrackInfo("id", string.Empty, string.Empty);
            var hashes = GetHashes(1000);

            modelReferenceTracker.AssignModelReferences(track, hashes);
            
            Assert.IsFalse(modelReferenceTracker.TryResetTrackRef(1));
            Assert.IsTrue(modelReferenceTracker.TryResetTrackRef(2));
            Assert.IsFalse(modelReferenceTracker.TryResetSubFingerprintRef(1000));
            Assert.IsTrue(modelReferenceTracker.TryResetSubFingerprintRef(1001));
        }
        
        [Test]
        public void ShouldAssignModelReferences()
        {
            var modelReferenceTracker = new UIntModelReferenceTracker();
            int numberOfHashBins = 100;
            var trackRefs = new ConcurrentBag<uint>();
            var subFingerprintRefs = new ConcurrentBag<uint>();
            Parallel.For(0, 1000, _ =>
            {
                var trackInfo = new TrackInfo(Guid.NewGuid().ToString(), string.Empty, string.Empty);
                var hashes = GetHashes(numberOfHashBins);

                var (trackData, subFingerprints) = modelReferenceTracker.AssignModelReferences(trackInfo, hashes);
                
                trackRefs.Add((uint)trackData.TrackReference.Id);
                foreach (uint subId in subFingerprints.Select(data => (uint)data.SubFingerprintReference.Id))
                {
                    subFingerprintRefs.Add(subId);
                }
            });
            
            Assert.AreEqual(trackRefs.Count, trackRefs.Distinct().Count());
            Assert.AreEqual(subFingerprintRefs.Count, subFingerprintRefs.Distinct().Count());
        }

        private static Hashes GetHashes(int numberOfHashBins)
        {
            return new Hashes(Enumerable
                .Range(0, numberOfHashBins)
                .Select(sequenceNumber => new HashedFingerprint(
                    new[]{255},
                    (uint) sequenceNumber,
                    sequenceNumber * 0.928f,
                    new byte[] {1})), (numberOfHashBins + 1) * 0.928f);
        }
    }
}