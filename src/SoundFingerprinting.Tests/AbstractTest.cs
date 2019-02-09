﻿namespace SoundFingerprinting.Tests
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public abstract class AbstractTest
    {
        private readonly int[] genericHashBucketsArray =
            {
                256, 770, 1284, 1798, 2312, 2826, 3340, 3854, 4368, 4882, 5396, 5910, 6424, 6938, 7452, 7966, 8480,
                9506, 10022, 10536, 11050, 11564, 12078, 12592, 13106
            };

        protected int[] GenericHashBuckets()
        {
            return (int[])genericHashBucketsArray.Clone();
        }

        protected void AssertTracksAreEqual(TrackInfo expectedTrack, TrackData actualTrack)
        {
            Assert.AreEqual(expectedTrack.Artist, actualTrack.Artist);
            Assert.AreEqual(expectedTrack.Title, actualTrack.Title);
            Assert.AreEqual(expectedTrack.DurationInSeconds, actualTrack.Length);
            Assert.AreEqual(expectedTrack.Id, actualTrack.ISRC);
            foreach (var metaField in expectedTrack.MetaFields)
            {
                Assert.IsTrue(actualTrack.MetaFields.TryGetValue(metaField.Key, out var value));
                Assert.AreEqual(metaField.Value, value);
            }
        }

        protected void AssertModelReferenceIsInitialized(IModelReference modelReference)
        {
            Assert.IsNotNull(modelReference);
            Assert.IsTrue(modelReference.GetHashCode() != 0);
        }
    }
}
