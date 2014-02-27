namespace SoundFingerprinting.MongoDb.Tests.Unit.Entity
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using MongoDB.Bson;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.MongoDb.Data;
    using SoundFingerprinting.Tests;

    [TestClass]
    public class TrackTest : AbstractTest
    {
        [TestMethod]
        public void MappingBetweenTrackDataAndTrackWorksCorrectly()
        {
            ObjectId id = ObjectId.GenerateNewId();
            TrackData trackData = new TrackData("isrc", "artist", "title", "album", 1986, 42, new MongoModelReference(id));

            var track = Track.FromTrackData(trackData);

            AssertTrackAreEqual(trackData, track);
        }

        [TestMethod]
        public void MappingBetweenTrackDataAndTrackWorksCorrectlyWithNullId()
        {
            TrackData trackData = new TrackData("isrc", "artist", "title", "album", 1986, 42, null);

            var track = Track.FromTrackData(trackData);

            AssertTrackAreEqual(trackData, track);
        }

        private void AssertTrackAreEqual(TrackData trackData, Track track)
        {
            Assert.AreEqual(trackData.ISRC, track.ISRC);
            Assert.AreEqual(trackData.Artist, track.Artist);
            Assert.AreEqual(trackData.Title, track.Title);
            Assert.AreEqual(trackData.Album, track.Album);
            Assert.AreEqual(trackData.ReleaseYear, track.ReleaseYear);
            Assert.AreEqual(trackData.TrackLengthSec, track.TrackLengthSec);

            if (trackData.TrackReference != null)
            {
                Assert.AreEqual(trackData.TrackReference.Id, track.Id);

            }
            else
            {
                Assert.AreEqual(ObjectId.Empty, track.Id);
            }
        }
    }
}
