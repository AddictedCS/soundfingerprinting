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
        public void ShouldThrowAsMaxValueExceeded()
        {
            Assert.Throws<ModelReferenceMaxAllowedValueExceededException>(() => new UIntModelReferenceTracker((uint)int.MaxValue + 1, 0, int.MaxValue));

            var modelReferenceTracker = new UIntModelReferenceTracker(int.MaxValue, int.MaxValue, int.MaxValue);
            
             var track = new TrackInfo("id", "title", "artist", new Dictionary<string, string>(){{"key", "value"}}, MediaType.Audio | MediaType.Video);
             var hashes = GetHashes(1);

             Assert.Throws<ModelReferenceMaxAllowedValueExceededException>(() => modelReferenceTracker.AssignModelReferences(track, hashes));
        }
        
        [Test]
        public void ShouldCopyAllFieldsCorrectly()
        {
            var modelReferenceTracker = new UIntModelReferenceTracker(0, 0);
            var track = new TrackInfo("id", "title", "artist", new Dictionary<string, string>(){{"key", "value"}}, MediaType.Audio | MediaType.Video);
            var hashes = GetHashes(1);

            var (trackData, subFingerprints) = modelReferenceTracker.AssignModelReferences(track, hashes);
            
            AssertTracksAreEqual(track, trackData);
            var enumerable = subFingerprints as SubFingerprintData[] ?? subFingerprints.ToArray();
			Assert.That(enumerable.Count(), Is.EqualTo(1));

            var hash = hashes.First();
            var subFingerprintData = enumerable.First();
			Assert.That(subFingerprintData.Hashes, Is.EqualTo(hash.HashBins).AsCollection);
			Assert.That(subFingerprintData.OriginalPoint, Is.EqualTo(hash.OriginalPoint).AsCollection);
			Assert.Multiple(() =>
			{
				Assert.That(subFingerprintData.SequenceNumber, Is.EqualTo(hash.SequenceNumber));
				Assert.That(subFingerprintData.SequenceAt, Is.EqualTo(hash.StartsAt));
			});
		}
        
        [Test]
        public void ShouldResetModelReferences()
        {
            var modelReferenceTracker = new UIntModelReferenceTracker(0, 0);
            var track = new TrackInfo("id", string.Empty, string.Empty);
            var hashes = GetHashes(1000);

            modelReferenceTracker.AssignModelReferences(track, hashes);

			Assert.Multiple(() =>
			{
				Assert.That(modelReferenceTracker.TryResetTrackRef(1), Is.False);
				Assert.That(modelReferenceTracker.TryResetTrackRef(2), Is.True);
				Assert.That(modelReferenceTracker.TryResetSubFingerprintRef(1000), Is.False);
				Assert.That(modelReferenceTracker.TryResetSubFingerprintRef(1001), Is.True);
			});
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
                
                trackRefs.Add(trackData.TrackReference.Get<uint>());
                foreach (uint subId in subFingerprints.Select(data => data.SubFingerprintReference.Get<uint>()))
                {
                    subFingerprintRefs.Add(subId);
                }
            });

			Assert.Multiple(() =>
			{
				Assert.That(trackRefs.Distinct().Count(), Is.EqualTo(trackRefs.Count));
				Assert.That(subFingerprintRefs.Distinct().Count(), Is.EqualTo(subFingerprintRefs.Count));
			});
		}

        [Test]
        public void ShouldWrapBothCountersInsteadOfThrowing()
        {
            var tracker = new UIntModelReferenceTracker(maxAllowedReference: 10, wrapOnOverflow: true);
            var track = new TrackInfo("id", string.Empty, string.Empty);

            // drives the sub counter to 9 over three tracks of three hashes each
            for (int i = 0; i < 3; ++i)
            {
                tracker.AssignModelReferences(track, GetHashes(3));
            }

            Assert.That(tracker.State, Is.EqualTo(new UIntModelReferenceState(0, 3, 9)));

            // the fourth track needs sub ids 10, 11 and 12, so both counters restart at 0 first
            var (trackData, subFingerprints) = tracker.AssignModelReferences(track, GetHashes(3));

            Assert.Multiple(() =>
            {
                Assert.That(trackData.TrackReference.Get<uint>(), Is.EqualTo(1u));
                Assert.That(subFingerprints.Select(_ => _.SubFingerprintReference.Get<uint>()), Is.EqualTo(new[] { 1u, 2u, 3u }));
                Assert.That(tracker.State, Is.EqualTo(new UIntModelReferenceState(1, 1, 3)));
            });
        }

        [Test]
        public void ShouldNotStraddleTheWrap()
        {
            var tracker = new UIntModelReferenceTracker(maxAllowedReference: 10, wrapOnOverflow: true);
            var track = new TrackInfo("id", string.Empty, string.Empty);

            tracker.AssignModelReferences(track, GetHashes(5));

            // 5 + 8 exceeds the maximum, so the whole block lands contiguously in [1..8] after the wrap
            var (_, subFingerprints) = tracker.AssignModelReferences(track, GetHashes(8));

            Assert.That(subFingerprints.Select(_ => _.SubFingerprintReference.Get<uint>()), Is.EqualTo(Enumerable.Range(1, 8).Select(_ => (uint)_)));
        }

        [Test]
        public void ShouldThrowWhenOneTrackDoesNotFitAWholeLap()
        {
            var tracker = new UIntModelReferenceTracker(maxAllowedReference: 10, wrapOnOverflow: true);
            var track = new TrackInfo("id", string.Empty, string.Empty);

            Assert.Throws<ModelReferenceMaxAllowedValueExceededException>(() => tracker.AssignModelReferences(track, GetHashes(11)));
        }

        [Test]
        public void ShouldLeaveTheCountersUsableAfterARefusedAssignment()
        {
            var tracker = new UIntModelReferenceTracker(0, 0, 10);
            var track = new TrackInfo("id", string.Empty, string.Empty);

            tracker.AssignModelReferences(track, GetHashes(8));

            // the throwing path used to leave the sub counter at 11, above the maximum, and unusable after that
            Assert.Throws<ModelReferenceMaxAllowedValueExceededException>(() => tracker.AssignModelReferences(track, GetHashes(3)));
            Assert.That(tracker.State, Is.EqualTo(new UIntModelReferenceState(0, 2, 10)));
        }

        [Test]
        public void ShouldReportStateOfBothCounters()
        {
            var tracker = new UIntModelReferenceTracker(0, 0);
            var track = new TrackInfo("id", string.Empty, string.Empty);

            tracker.AssignModelReferences(track, GetHashes(7));
            tracker.TryResetSubFingerprintRef(100);

            Assert.That(tracker.State, Is.EqualTo(new UIntModelReferenceState(0, 1, 100)));
        }

        private static Hashes GetHashes(int numberOfHashBins)
        {
            return new Hashes(Enumerable
                .Range(0, numberOfHashBins)
                .Select(sequenceNumber => 
                    new HashedFingerprint(new[]{255}, (uint) sequenceNumber, sequenceNumber * 0.928f, new byte[] {1})), (numberOfHashBins + 1) * 0.928f, MediaType.Audio);
        }
    }
}