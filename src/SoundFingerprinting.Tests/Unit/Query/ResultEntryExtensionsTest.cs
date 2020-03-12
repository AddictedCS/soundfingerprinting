namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using NUnit.Framework;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class ResultEntryExtensionsTest
    {
        [Test]
        public void ShouldNotMergeAsTracksDoNotMatch()
        {
            var a = new ResultEntry(new TrackData("123", "artist", "title", "album", 0, 120d, new ModelReference<int>(1)), 0.9d, 120, DateTime.Now, 10d, 0d, 10, 10, 5d, -5d);
            
            var b = new ResultEntry(new TrackData("123", "artist", "title", "album", 0, 120d, new ModelReference<int>(2)), 0.9d, 120, DateTime.Now, 10d, 0d, 10, 10, 5d, -5d);

            Assert.Throws<ArgumentException>(() => a.MergeWith(b));
        }
        
        [Test]
        public void ShouldMergeCorrectly1()
        {
            /*
             * t -----------
             * a ------
             * b      ------
             */
            var data = new TrackData("123", "artist", "title", "album", 0, 120d, new ModelReference<int>(1));
            var a = new ResultEntry(data, 0.9d, 120, DateTime.Now, 10d, 0d, 10, 10, 5d, -5d);
            var b = new ResultEntry(data, 0.9d, 120, DateTime.Now, 10d, 0d, 7, 7, 13d, -13d);

            var merged = a.MergeWith(b);

            Assert.AreEqual(15, merged.CoverageWithPermittedGapsLength);
            Assert.AreEqual(15, merged.DiscreteCoverageLength);
            Assert.AreEqual(18, merged.QueryLength);
            Assert.AreEqual(5d, merged.TrackMatchStartsAt);
            Assert.AreEqual(-5d, merged.TrackStartsAt);
        }
        
        [Test]
        public void ShouldMergeCorrectly2()
        {
            /*
             * t ----------------
             * a ------
             * b           ------
             */
            var data = new TrackData("123", "artist", "title", "album", 0, 120d, new ModelReference<int>(1));
            var a = new ResultEntry(data, 0.9d, 120, DateTime.Now, 15d, 0d, 10, 10, 5d, -5d);
            var b = new ResultEntry(data, 0.9d, 120, DateTime.Now, 15d, 0d, 10, 10, 30d, -30d);

            var merged = a.MergeWith(b);

            Assert.AreEqual(20, merged.CoverageWithPermittedGapsLength);
            Assert.AreEqual(20, merged.DiscreteCoverageLength);
            Assert.AreEqual(30, merged.QueryLength);
            Assert.AreEqual(5d, merged.TrackMatchStartsAt);
            Assert.AreEqual(-5d, merged.TrackStartsAt);
        }
        
        [Test]
        public void ShouldMergeCorrectly3()
        {
            /*
             * t ----------------
             * a ----------
             * b   ------
             */
            var data = new TrackData("123", "artist", "title", "album", 0, 120d, new ModelReference<int>(1));
            var a = new ResultEntry(data, 0.9d, 120, DateTime.Now, 15d, 0d, 10, 15, 15d, -15d);
            var b = new ResultEntry(data, 0.9d, 120, DateTime.Now, 10d, 0d, 10, 5, 20d, -20d);

            var merged = a.MergeWith(b);

            Assert.AreEqual(15, merged.CoverageWithPermittedGapsLength);
            Assert.AreEqual(15, merged.DiscreteCoverageLength);
            Assert.AreEqual(15, merged.QueryLength);
            Assert.AreEqual(15d, merged.TrackMatchStartsAt);
            Assert.AreEqual(-15d, merged.TrackStartsAt);
        }
    }
}