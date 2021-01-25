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
            double permittedGap = 1d;
            var track = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));
            
            var entry1 = new PendingResultEntry(new ResultEntry(track, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {1, 2, 3}).EstimateCoverage(3, 9, 1, permittedGap)));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {4, 5, 6}).EstimateCoverage(3, 9, 1, permittedGap)));
            var entry3 = new PendingResultEntry(new ResultEntry(track, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {7, 8, 9}).EstimateCoverage(3, 9, 1, permittedGap)));

            Assert.IsTrue(entry1.TryCollapse(entry2, permittedGap, out var collapsed1));
            Assert.IsTrue(collapsed1.TryCollapse(entry3, permittedGap, out var collapsed2));

            var result = collapsed2.Entry;
            Assert.AreEqual(9, result.TrackCoverageWithPermittedGapsLength, 0.0001);
            Assert.AreEqual(9, result.QueryLength, 0.0001);
        }

        [Test]
        public void ShouldNotCollapseAsTracksAreDifferent()
        {
            double permittedGap = 1d;
            var track1 = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));
            var track2 = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(2));
            
            var entry1 = new PendingResultEntry(new ResultEntry(track1, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {1, 2, 3}).EstimateCoverage(3, 9, 1, permittedGap)));
            var entry2 = new PendingResultEntry(new ResultEntry(track2, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {4, 5, 6}).EstimateCoverage(3, 9, 1, permittedGap)));

            Assert.IsFalse(entry1.TryCollapse(entry2, permittedGap, out _));
        }

        [Test]
        public void ShouldNotCollapseAsTheStretchBetweenMatchesIsTooLong()
        {
            double permittedGap = 1d;
            var track1 = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track1, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {1, 2, 3}).EstimateCoverage(3, 9, 1, permittedGap)));
            var entry2 = new PendingResultEntry(new ResultEntry(track1, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {10, 11, 12}).EstimateCoverage(3, 9, 1, permittedGap)));
            
            Assert.IsFalse(entry1.TryCollapse(entry2, permittedGap, out _));
        }

        [Test]
        public void IsCompletedAsTheMatchIsLongerThanTheThreshold()
        {
            double permittedGap = 3d;
            var track = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {1, 2, 3}).EstimateCoverage(3, 9, 1, permittedGap)));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {6, 7, 8}).EstimateCoverage(3, 9, 1, permittedGap)));
            
            entry1.TryCollapse(entry2, permittedGap, out var collapsed);

            Assert.AreEqual(8, collapsed.Entry.TrackCoverageWithPermittedGapsLength);
            Assert.AreEqual(8, collapsed.Entry.DiscreteTrackCoverageLength);
        }
        
        [Test]
        public void ShouldSwallowEntryWithinEntry()
        {
            double permittedGap = 3d;
            var track = new TrackData("1", "artist", "title", 120, new ModelReference<uint>(1));

            var entry1 = new PendingResultEntry(new ResultEntry(track, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3, 4, 5, 6, 7}, new[] {1, 2, 3, 4, 5, 6, 7}).EstimateCoverage(7, 7, 1, permittedGap)));
            var entry2 = new PendingResultEntry(new ResultEntry(track, 100, DateTime.Now, TestUtilities.GetMatchedWith(new[] {1, 2, 3}, new[] {3, 4, 5}).EstimateCoverage(3, 7, 1, permittedGap)));
            
            Assert.IsTrue(entry1.TryCollapse(entry2, permittedGap, out var collapsed));

            Assert.AreEqual(7, collapsed.Entry.TrackCoverageWithPermittedGapsLength); 
        }
    }
}