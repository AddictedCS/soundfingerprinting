namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System;
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
            var qrc = new QueryResultCoverageCalculator();

            var track = new TrackData("1234", "artist", "title", 120d, new ModelReference<int>(1));

            var coverages = qrc.GetCoverages(track, new GroupedQueryResults(10d, DateTime.Now), new DefaultQueryConfiguration());
            
            Assert.IsFalse(coverages.Any());
        }
    }
}