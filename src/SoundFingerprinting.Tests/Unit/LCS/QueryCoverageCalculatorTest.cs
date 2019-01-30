namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class QueryCoverageCalculatorTest
    {
        [Test]
        public void ShouldNotFailWithEmptyEntries()
        {
            var qrc = new QueryResultCoverageCalculator(new LongestIncreasingTrackSequence());

            var track = new TrackData("1234", "artist", "title", "album", 1986, 120d, new ModelReference<int>(1));

            var coverages = qrc.GetCoverages(track, new GroupedQueryResults(10d), new DefaultQueryConfiguration());
            
            Assert.IsFalse(coverages.Any());
        }
    }
}