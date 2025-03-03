namespace SoundFingerprinting.Tests.Unit.Command;

using System;
using NUnit.Framework;
using SoundFingerprinting.Command;
using SoundFingerprinting.DAO;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Query;

[TestFixture]
public class TrackRelativeCoverageEntryFilterTest
{
    [TestCase(0.4, true, 0.2, true, false)]
    [TestCase(0.4, true, 0.5, true, false)]
    [TestCase(0.4, true, 0.7, true, false)]
    [TestCase(0.4, true, 0.9, false, true)]
    [TestCase(0.4, true, 0.3, false, false)]
    [TestCase(0.4, true, 0.5, false, true)]
    public void ShouldWorkAsExpected(double relativeCoverage, bool waitTillCompletion, double runningCoverage, bool canContinueInTheNextQuery, bool expected)
    {
        var filter = new TrackRelativeCoverageEntryFilter(relativeCoverage, waitTillCompletion);
        var track = new TrackData("id", "author", "title", length: 30, new ModelReference<uint>(1));
        var coverage = TestUtilities.GetCoverage(track.Length * runningCoverage, queryLength: 30, trackLength: 30, []);
        var resultEntry = new ResultEntry(track, 1, DateTime.UnixEpoch, coverage);
        
        bool actual = filter.Pass(new AVResultEntry(resultEntry, resultEntry), canContinueInTheNextQuery);
        
        Assert.That(actual, Is.EqualTo(expected));
    }
}