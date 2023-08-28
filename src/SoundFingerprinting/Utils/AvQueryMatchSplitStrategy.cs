namespace SoundFingerprinting.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using SoundFingerprinting.Data;
using SoundFingerprinting.Query;

/// <summary>
///  Class that splits query matches according to given length and original coverage.
/// </summary>
public class AvQueryMatchSplitStrategy
{
    /// <summary>
    ///  Splits the query match according to the given length and original coverage.
    /// </summary>
    /// <param name="avQueryMatch">AV query match to split.</param>
    /// <param name="tracks">Newly created tracks that will be associated with split av query matches.</param>
    /// <param name="timestamps">Timestamps in seconds where to split the matches.</param>
    /// <param name="trackCoverageThreshold">Relative track coverage to filter false positives.</param>
    /// <returns>
    ///   Split av query matches.
    /// </returns>
    public static IEnumerable<AVQueryMatch> Split(AVQueryMatch avQueryMatch, TrackInfo[] tracks, double[] timestamps, double trackCoverageThreshold)
    {
        if (tracks.Length != timestamps.Length)
        {
            throw new ArgumentException("Tracks and timestamps must have the same length", nameof(tracks));
        }

        if (trackCoverageThreshold is < 0 or > 1)
        {
            throw new ArgumentException($"{nameof(trackCoverageThreshold)} should be between [0,1]");
        }
        
        var (audio, video) = avQueryMatch;

        var audioQueryMatches = Split(audio, tracks, timestamps, trackCoverageThreshold);
        var videoQueryMatches = Split(video, tracks, timestamps, trackCoverageThreshold);
        var split = audioQueryMatches.Zip(videoQueryMatches, (a, b) =>
            {
                if (a == null && b == null)
                {
                    return null;
                }

                return new AVQueryMatch(Guid.NewGuid().ToString(), a, b, avQueryMatch.StreamId, avQueryMatch.ReviewStatus);
            })
            .Where(x => x != null)
            .Select(x => x!);
        
        return split;
    }

    private static IEnumerable<QueryMatch?> Split(QueryMatch? queryMatch, TrackInfo[] tracks, double[] timestamps, double trackCoverageThreshold)
    {
        // find the original relative to
        var relativeTo = queryMatch?.MatchedAt.Subtract(TimeSpan.FromSeconds(queryMatch?.Coverage.QueryMatchStartsAt ?? 0)) ?? DateTime.UtcNow;
        
        // split the coverage
        return queryMatch?
            .Coverage
            .SplitByTrackLength(timestamps)
            .Select((coverage, index) =>
            {
                if (!coverage.BestPath.Any())
                {
                    // empty coverage, no match
                    return null;
                }
                
                if (coverage.TrackCoverageWithPermittedGapsLength / coverage.TrackLength < trackCoverageThreshold)
                {
                    // coverage is too small, no match
                    return null;
                }
                
                // return a new query match with the new coverage and adjusted relative to
                return new QueryMatch(Guid.NewGuid().ToString(), tracks[index], coverage, relativeTo.AddSeconds(coverage.QueryMatchStartsAt));
            }) ?? Enumerable.Repeat((QueryMatch?)null, timestamps.Length);
    }
}