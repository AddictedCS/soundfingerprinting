namespace SoundFingerprinting.Command;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.Data;
using SoundFingerprinting.Query;

internal class NoOpQueryHashesConcatenator : IQueryHashesConcatenator
{
    private readonly ILogger<NoOpQueryHashesConcatenator> logger;

    public NoOpQueryHashesConcatenator(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<NoOpQueryHashesConcatenator>();
    }
    
    public void UpdateHashesForTracks(IEnumerable<string> trackIds, AVHashes hashes, AVQueryCommandStats commandStats)
    {
        logger.LogDebug("{FieldName} is set to False, no query hashes will be included in the response", nameof(RealtimeQueryConfiguration.IncludeQueryHashesInResponse));
    }

    public IEnumerable<AVQueryResult> GetQueryResults(IEnumerable<AVResultEntry> completed)
    {
        var avResultEntries = completed.ToList();
        var audio = new QueryResult(avResultEntries.Select(_ => _.Audio).Where(entry => entry != null)!, Hashes.GetEmpty(MediaType.Audio), QueryCommandStats.Zero());
        var video = new QueryResult(avResultEntries.Select(_ => _.Video).Where(entry => entry != null)!, Hashes.GetEmpty(MediaType.Video), QueryCommandStats.Zero());
        return new[] { new AVQueryResult(audio, video, AVHashes.Empty, new AVQueryCommandStats(QueryCommandStats.Zero(), QueryCommandStats.Zero())) };
    }

    public void Cleanup(IEnumerable<string> purgedTrackIds)
    {
        logger.LogDebug("{FieldName} is set to False, no query hashes will be included in the response", nameof(RealtimeQueryConfiguration.IncludeQueryHashesInResponse));
    }
}