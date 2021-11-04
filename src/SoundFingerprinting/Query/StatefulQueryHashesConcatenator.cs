namespace SoundFingerprinting.Query
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    public class StatefulQueryHashesConcatenator : IQueryHashesConcatenator
    {
        private readonly ConcurrentDictionary<string, AVHashes> trackQueryHashes = new ();
        private readonly ConcurrentDictionary<AVHashes, AVQueryCommandStats> trackQueryStats = new ();
        
        public void UpdateHashesForTracks(IEnumerable<string> trackIds, AVHashes hashes, AVQueryCommandStats commandStats)
        {
            var merged = new Dictionary<AVHashes, AVHashes>();
            foreach (var trackId in trackIds)
            {
                // update ongoing track match with merged query hashes.
                trackQueryHashes.AddOrUpdate(trackId, _ =>
                {
                    // if never seen before, insert hashes and query stats corresponding to hashes.
                    trackQueryStats[hashes] = commandStats;
                    return hashes;
                },
                (_, prev) =>
                {
                    if (merged.ContainsKey(prev))
                    {
                        // we have previously merged these hashes, let's return previous key, making sure we don't merge again
                        return merged[prev];
                    }

                    var mergedHashes = merged[prev] = prev.MergeWith(hashes);

                    // update query stats information
                    trackQueryStats.TryRemove(prev, out var stats);
                    trackQueryStats[mergedHashes] = stats.Sum(commandStats);
                    return mergedHashes;
                });
            }
        }
        
        public IEnumerable<AVQueryResult> GetQueryResults(IEnumerable<AVResultEntry> completed)
        {
            return completed
                .Join(trackQueryHashes, _ => _.Track.Id, _ => _.Key, (p1, p2) => new { ResultEntry = p1, Hashes = p2.Value })
                .GroupBy(_ => _.Hashes)
                .Select(group =>
                {
                    var entries = @group.Select(_ => _.ResultEntry).ToList();
                    var stats = trackQueryStats[@group.Key];
                    return new AVQueryResult(entries, @group.Key, stats);
                })
                .ToList();
        }
        
        public void Cleanup(IEnumerable<string> purgedTrackIds)
        {
            foreach (var entry in purgedTrackIds)
            {
                if (trackQueryHashes.TryRemove(entry, out var hashes))
                {
                    // is anyone referencing hashes same hashes object?
                    // could happen when a match can continue in next query and a match that is purged is generated from the same query hashes
                    if (!trackQueryHashes.Select(_ => _.Value).Any(h => h.Equals(hashes)))
                    {
                        trackQueryStats.TryRemove(hashes, out _);
                    }
                }
            }
        }
    }
}