namespace SoundFingerprinting.Tests.Unit.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.DAO;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Query;

[TestFixture]
public class QueryMathTest
{
    private readonly QueryMath queryMath = QueryMath.Instance;

    [Test]
    public void ShouldReturnEmptyListWhenNoTracks()
    {
        var groupedQueryResults = new GroupedQueryResults(10d, DateTime.Now, queryProfile: null);
        var queryService = new Mock<IQueryService>();
        queryService
            .Setup(s => s.ReadTracksByReferences(It.IsAny<IEnumerable<IModelReference>>()))
            .Returns([]);

        var results = queryMath.GetBestCandidates(groupedQueryResults, 10, queryService.Object, new DefaultQueryConfiguration());

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void ShouldReturnResultsForSingleTrack()
    {
        var trackRef = new ModelReference<uint>(1);
        var trackData = new TrackData("id-1", "Artist", "Title", 30.0, trackRef);
        var groupedQueryResults = CreateGroupedQueryResultsWithMatches(10d, trackRef, 5);

        var queryService = new Mock<IQueryService>();
        queryService
            .Setup(s => s.ReadTracksByReferences(It.IsAny<IEnumerable<IModelReference>>()))
            .Returns([trackData]);

        var results = queryMath.GetBestCandidates(groupedQueryResults, 10, queryService.Object, new DefaultQueryConfiguration());

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.All(r => r.Track.TrackReference.Equals(trackRef)), Is.True);
    }

    [Test]
    public void ShouldReturnResultsForTwoTracksUsingSequentialPath()
    {
        var trackRef1 = new ModelReference<uint>(1);
        var trackRef2 = new ModelReference<uint>(2);
        var trackData1 = new TrackData("id-1", "Artist1", "Title1", 30.0, trackRef1);
        var trackData2 = new TrackData("id-2", "Artist2", "Title2", 30.0, trackRef2);
        var groupedQueryResults = CreateGroupedQueryResultsWithMatches(10d, [trackRef1, trackRef2], 5);

        var queryService = new Mock<IQueryService>();
        queryService
            .Setup(s => s.ReadTracksByReferences(It.IsAny<IEnumerable<IModelReference>>()))
            .Returns([trackData1, trackData2]);

        var results = queryMath.GetBestCandidates(groupedQueryResults, 10, queryService.Object, new DefaultQueryConfiguration());

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void ShouldReturnResultsForMultipleTracksUsingParallelPath()
    {
        // Create more than 2 tracks to trigger parallel processing
        var trackRefs = Enumerable.Range(1, 5).Select(i => new ModelReference<uint>((uint)i)).ToArray();
        var trackDataList = trackRefs.Select((tr, i) => new TrackData($"id-{i}", $"Artist{i}", $"Title{i}", 30.0, tr)).ToArray();
        var groupedQueryResults = CreateGroupedQueryResultsWithMatches(10d, trackRefs.Cast<IModelReference>().ToArray(), 5);

        var queryService = new Mock<IQueryService>();
        queryService
            .Setup(s => s.ReadTracksByReferences(It.IsAny<IEnumerable<IModelReference>>()))
            .Returns(trackDataList);

        var results = queryMath.GetBestCandidates(groupedQueryResults, 10, queryService.Object, new DefaultQueryConfiguration());

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(5));

        // Verify results are ordered by score descending
        var scores = results.Select(r => r.Score).ToList();
        Assert.That(scores, Is.Ordered.Descending);
    }

    [Test]
    public void ShouldReturnResultsInDescendingScoreOrder()
    {
        var trackRef1 = new ModelReference<uint>(1);
        var trackRef2 = new ModelReference<uint>(2);
        var trackRef3 = new ModelReference<uint>(3);
        var trackData1 = new TrackData("id-1", "Artist1", "Title1", 30.0, trackRef1);
        var trackData2 = new TrackData("id-2", "Artist2", "Title2", 30.0, trackRef2);
        var trackData3 = new TrackData("id-3", "Artist3", "Title3", 30.0, trackRef3);

        // Create grouped results with different scores for each track
        var groupedQueryResults = new GroupedQueryResults(10d, DateTime.Now, queryProfile: null);
        AddMatchesForTrack(groupedQueryResults, trackRef1, 5, 100);  // Score: 500
        AddMatchesForTrack(groupedQueryResults, trackRef2, 5, 200);  // Score: 1000
        AddMatchesForTrack(groupedQueryResults, trackRef3, 5, 150);  // Score: 750

        var queryService = new Mock<IQueryService>();
        queryService
            .Setup(s => s.ReadTracksByReferences(It.IsAny<IEnumerable<IModelReference>>()))
            .Returns([trackData1, trackData2, trackData3]);

        var results = queryMath.GetBestCandidates(groupedQueryResults, 10, queryService.Object, new DefaultQueryConfiguration());

        Assert.That(results, Is.Not.Empty);

        // Verify results are ordered by score descending
        var scores = results.Select(r => r.Score).ToList();
        Assert.That(scores, Is.Ordered.Descending);
    }

    [Test]
    public void ShouldHandleManyTracksInParallel()
    {
        // Create many tracks to stress test parallel processing
        const int trackCount = 20;
        var trackRefs = Enumerable.Range(1, trackCount).Select(i => new ModelReference<uint>((uint)i)).ToArray();
        var trackDataList = trackRefs.Select((tr, i) => new TrackData($"id-{i}", $"Artist{i}", $"Title{i}", 30.0, tr)).ToArray();
        var groupedQueryResults = CreateGroupedQueryResultsWithMatches(10d, trackRefs.Cast<IModelReference>().ToArray(), 10);

        var queryService = new Mock<IQueryService>();
        queryService
            .Setup(s => s.ReadTracksByReferences(It.IsAny<IEnumerable<IModelReference>>()))
            .Returns(trackDataList);

        var results = queryMath.GetBestCandidates(groupedQueryResults, trackCount, queryService.Object, new DefaultQueryConfiguration());

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(trackCount));

        // Verify results are ordered by score descending
        var scores = results.Select(r => r.Score).ToList();
        Assert.That(scores, Is.Ordered.Descending);

        // Verify all tracks have at least one result
        var resultTrackRefs = results.Select(r => r.Track.TrackReference).Distinct().ToList();
        Assert.That(resultTrackRefs.Count, Is.EqualTo(trackCount));
    }

    private static GroupedQueryResults CreateGroupedQueryResultsWithMatches(double queryLength, IModelReference trackRef, int matchCount)
    {
        return CreateGroupedQueryResultsWithMatches(queryLength, [trackRef], matchCount);
    }

    private static GroupedQueryResults CreateGroupedQueryResultsWithMatches(double queryLength, IModelReference[] trackRefs, int matchCount)
    {
        var groupedQueryResults = new GroupedQueryResults(queryLength, DateTime.Now, queryProfile: null);
        foreach (var trackRef in trackRefs)
        {
            AddMatchesForTrack(groupedQueryResults, trackRef, matchCount, 100);
        }

        return groupedQueryResults;
    }

    private static void AddMatchesForTrack(GroupedQueryResults groupedQueryResults, IModelReference trackRef, int matchCount, double score)
    {
        for (uint i = 0; i < matchCount; i++)
        {
            var matchedWith = new MatchedWith(i, i * 1.0f, i, i * 1.0f, score);
            groupedQueryResults.Add(i, trackRef, matchedWith);
        }
    }
}