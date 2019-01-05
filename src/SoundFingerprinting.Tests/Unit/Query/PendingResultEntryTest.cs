namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using NUnit.Framework;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class PendingResultEntryTest
    {
        [Test]
        public void ShouldCollapse()
        {
            var track = new TrackData("isrc", "artist", "title", string.Empty, 0, 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 0d, 1.48d, 1.48d, 10d, -10, 0d, 100, 1.48d));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 0d, 1.48d, 1.48d, 10d + 1.48d, -10 + 1.48, 0d, 100, 1.48d));
            var entry3 = new PendingResultEntry(new ResultEntry(track, 0d, 1.48d, 1.48d, 10d + 2*1.48d, -10 + 2*1.48, 0d, 100, 1.48d));

            Assert.IsTrue(entry1.TryCollapse(entry2, out var collapsed1));
            Assert.IsTrue(collapsed1.TryCollapse(entry3, out var collapsed2));

            var result = collapsed2.Entry;
            Assert.AreEqual(3* 1.48, result.QueryMatchLength);
            Assert.AreEqual(3*1.48, result.QueryLength);
        }

        [Test]
        public void ShouldNotCollapseAsTracksAreDifferent()
        {
            var track1 = new TrackData("isrc", "artist", "title", String.Empty, 0, 120, new ModelReference<uint>(1));
            var track2 = new TrackData("isrc", "artist", "title", String.Empty, 0, 120, new ModelReference<uint>(2));
            
            var entry1 = new PendingResultEntry(new ResultEntry(track1, 0d, 1.48d, 1.48d, 10d, -10, 0d, 100, 1.48d));
            var entry2 = new PendingResultEntry(new ResultEntry(track2, 0d, 1.48d, 1.48d, 10d + 1.48d, -10 + 1.48, 0d, 100, 1.48d));

            Assert.IsFalse(entry1.TryCollapse(entry2, out _));
        }

        [Test]
        public void ShouldNotCollapseAsTheStretchBetweenMatchesIsTooLong()
        {
            var track = new TrackData("isrc", "artist", "title", string.Empty, 0, 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 0d, 1.48d, 1.48d, 10d, -10, 0d, 100, 1.48d));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 0d, 1.48d, 1.48d, 30d, -10 + 1.48, 0d, 100, 1.48d));
            
            Assert.IsFalse(entry1.TryCollapse(entry2, out _));
        }

        [Test]
        public void IsCompletedAsTheMatchIsLongerThanTheThreshold()
        {
            var track = new TrackData("isrc", "artist", "title", string.Empty, 0, 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 0d, 3d, 3d, 10d, -10, 0d, 100, 3d));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 0d, 3d, 3d, 14d, -10 + 3, 0d, 100, 3d));

            entry1.TryCollapse(entry2, out var collapsed);

            Assert.IsTrue(collapsed.IsCompleted(5d));
            Assert.AreEqual(6, collapsed.Entry.QueryMatchLength);
        }
    }
}