namespace SoundFingerprinting.Tests.Unit.Command;

using System;
using NUnit.Framework;
using SoundFingerprinting.Command;
using SoundFingerprinting.DAO;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Query;

[TestFixture]
public class ChainedRealtimeEntryFilterTest
{
    [TestCase(0.2, true, false)]
    [TestCase(0.4, true, false)]
    [TestCase(0.4, false, true)]
    [TestCase(0.6, false, true)]
    public void ShouldWorkAsExpected(double runnigCoverage, bool canContinueInTheNextQuery, bool expected)
    {
        var filter1 = new TrackRelativeCoverageEntryFilter(0.5, waitTillCompletion: true);
        var filter2 = new TrackCoverageLengthEntryFilter(10d, waitTillCompletion: true);
        var filter3 = new ChainedRealtimeEntryFilter([filter1, filter2]);
        
        var track = new TrackData("id", "isrc", "title", 30, new ModelReference<uint>(1));
        var coverage = TestUtilities.GetCoverage(track.Length * runnigCoverage, []);
        var resultEntry = new ResultEntry(track, 1, DateTime.UnixEpoch, coverage);
        
        bool actual = filter3.Pass(new AVResultEntry(resultEntry, resultEntry), canContinueInTheNextQuery);
        
        Assert.That(actual, Is.EqualTo(expected));
    }
}