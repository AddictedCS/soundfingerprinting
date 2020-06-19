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
            var track = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 1.48d, 0d, 1.48d, 1.48d, 10d, -10));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 1.48d, 0d, 1.48d, 1.48d, 10d + 1.48d, -10 + 1.48));
            var entry3 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 1.48d, 0d, 1.48d, 1.48d, 10d + 2 * 1.48d, -10 + 2 * 1.48));

            Assert.IsTrue(entry1.TryCollapse(entry2, 1.48, out var collapsed1));
            Assert.IsTrue(collapsed1.TryCollapse(entry3, 1.48, out var collapsed2));

            var result = collapsed2.Entry;
            Assert.AreEqual(3 * 1.48, result.CoverageWithPermittedGapsLength, 0.0001);
            Assert.AreEqual(3 * 1.48, result.QueryLength, 0.0001);
        }

        [Test]
        public void ShouldNotCollapseAsTracksAreDifferent()
        {
            var track1 = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));
            var track2 = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(2));
            
            var entry1 = new PendingResultEntry(new ResultEntry(track1, 0d, 100, DateTime.Now, 1.48d, 0d, 1.48d, 1.48d, 10d, -10));
            var entry2 = new PendingResultEntry(new ResultEntry(track2, 0d, 100, DateTime.Now, 1.48d, 0d, 1.48d, 1.48d, 10d + 1.48d, -10 + 1.48));

            Assert.IsFalse(entry1.TryCollapse(entry2, 1.48, out _));
        }

        [Test]
        public void ShouldNotCollapseAsTheStretchBetweenMatchesIsTooLong()
        {
            var track = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 1.48d, 0d, 1.48d, 1.48d, 10d, -10));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 1.48d, 0d, 1.48d, 1.48d, 30d, -10 + 1.48));
            
            Assert.IsFalse(entry1.TryCollapse(entry2, 1.48, out _));
        }

        [Test]
        public void IsCompletedAsTheMatchIsLongerThanTheThreshold()
        {
            var track = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 3d, 0d, 3d, 3d, 10d, -10));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 3d, 0d, 3d, 3d, 12d, -10 + 2));

            entry1.TryCollapse(entry2, 1.48, out var collapsed);

            Assert.AreEqual(5, collapsed.Entry.CoverageWithPermittedGapsLength);
        }
        
        [Test]
        public void IsCompletedAsTheMatchIsLongerThanTheThreshold2()
        {
            var track = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 3d, 0d, 3d, 3d, 10d, -10));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 3d, 0d, 3d, 3d, 14d, -10 + 2));

            entry1.TryCollapse(entry2, 1.48, out var collapsed);

            Assert.AreEqual(6, collapsed.Entry.CoverageWithPermittedGapsLength);
        }

        [Test]
        public void ShouldSwallowEntryWithinEntry()
        {
            var track = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 3d, 0d, 3d, 3d, 10d, -10));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 0d, 100, DateTime.Now, 2.5d, 0d, 2.5d, 2.5d, 10.5d, -10 + 2.5));

            Assert.IsTrue(entry1.TryCollapse(entry2, 1.48, out var collapsed));

            Assert.AreEqual(3, collapsed.Entry.CoverageWithPermittedGapsLength); 
        }
    }
}