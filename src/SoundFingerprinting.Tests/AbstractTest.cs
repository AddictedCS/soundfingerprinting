namespace SoundFingerprinting.Tests
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    public abstract class AbstractTest
    {
        protected const double Epsilon = 0.0001;

        protected const int SampleRate = 5512;

        protected const int NumberOfHashTables = 25;

        private readonly bool[] genericFingerprintArray = {
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true, true, false, true, false, true, false, true, false,
                true, false, true, false, false, true, false, true, false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true, true, false, true, false, true, false, true, false,
                true, false, true, false, false, true, false, true, false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true
            };

        private readonly int[] genericHashBucketsArray = {
                256, 770, 1284, 1798, 2312, 2826, 3340, 3854, 4368, 4882, 5396, 5910, 6424, 6938, 7452, 7966, 8480, 9506,
                10022, 10536, 11050, 11564, 12078, 12592, 13106
            };

        protected bool[] GenericFingerprint()
        {
            return (bool[])genericFingerprintArray.Clone();
        }

        protected int[] GenericHashBuckets()
        {
            return (int[]) genericHashBucketsArray.Clone();
        }

        protected void AssertTracksAreEqual(TrackData expectedTrack, TrackData actualTrack)
        {
            Assert.AreEqual(
                expectedTrack.TrackReference,
                actualTrack.TrackReference,
                $"Expected: {expectedTrack.TrackReference.Id}, Actual: {actualTrack.TrackReference.Id}");
            Assert.AreEqual(expectedTrack.Album, actualTrack.Album);
            Assert.AreEqual(expectedTrack.Artist, actualTrack.Artist);
            Assert.AreEqual(expectedTrack.Title, actualTrack.Title);
            Assert.AreEqual(expectedTrack.Length, actualTrack.Length);
            Assert.AreEqual(expectedTrack.ISRC, actualTrack.ISRC);
        }

        protected void AssertModelReferenceIsInitialized(IModelReference modelReference)
        {
            Assert.IsNotNull(modelReference);
            Assert.IsTrue(modelReference.GetHashCode() != 0);
        }
    }
}
