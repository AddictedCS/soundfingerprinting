namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    internal class StatefulQueryHashesConcatenator : IQueryHashesConcatenator
    {
        private readonly ConcurrentDictionary<string, AvQueryHashes> trackQueryHashes = new ();
        
        public void UpdateHashesForTracks(IEnumerable<string> trackIds, AVHashes hashes, AVQueryCommandStats commandStats)
        {
            var merged = new Dictionary<AVHashes, AvQueryHashes>();
            foreach (var trackId in trackIds)
            {
                // update ongoing track match with merged query hashes.
                trackQueryHashes.AddOrUpdate(trackId, _ => new AvQueryHashes(hashes, commandStats),
                    (_, prev) =>
                    {
                        var (prevHashes, prevStats) = prev;
                        if (merged.TryGetValue(prevHashes, out var prevMerged))
                        {
                            // we have previously merged these hashes, let's return previous key, making sure we don't merge again
                            return prevMerged;
                        }

                        return merged[prevHashes] = new AvQueryHashes(prevHashes.MergeWith(hashes), prevStats.Sum(commandStats));
                    });
            }
        }
        
        public IEnumerable<AVQueryResult> GetQueryResults(IEnumerable<AVResultEntry> completed)
        {
            return completed
                .Join(trackQueryHashes, _ => _.TrackId, _ => _.Key, (p1, p2) => new { ResultEntry = p1, Hashes = p2.Value })
                .GroupBy(_ => _.Hashes)
                .Select(group =>
                {
                    var entries = @group.Select(_ => _.ResultEntry).ToList();
                    var (hashes, stats) = group.Key;
                    var audioQueryMatch = new QueryResult(entries.Select(_ => _.Audio).Where(entry => entry != null)!, hashes.Audio ?? Hashes.GetEmpty(MediaType.Audio), stats.Audio ?? QueryCommandStats.Zero());
                    var videoQueryMatch = new QueryResult(entries.Select(_ => _.Video).Where(entry => entry != null)!, hashes.Video ?? Hashes.GetEmpty(MediaType.Video), stats.Video ?? QueryCommandStats.Zero());
                    return new AVQueryResult(audioQueryMatch, videoQueryMatch, entries, hashes, stats);
                })
                .ToList();
        }
        
        public void Cleanup(IEnumerable<string> purgedTrackIds)
        {
            foreach (var entry in purgedTrackIds)
            {
                trackQueryHashes.TryRemove(entry, out _);
            }
        }

        private class AvQueryHashes : IEquatable<AvQueryHashes>
        {
            public AvQueryHashes(AVHashes hashes, AVQueryCommandStats queryCommandStats)
            {
                Hashes = hashes;
                QueryCommandStats = queryCommandStats;
            }
            
            public AVHashes Hashes { get; }
            
            public AVQueryCommandStats QueryCommandStats { get; }

            public bool Equals(AvQueryHashes? other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Hashes.Equals(other.Hashes);
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((AvQueryHashes)obj);
            }

            public override int GetHashCode()
            {
                return Hashes.GetHashCode();
            }

            public void Deconstruct(out AVHashes hashes, out AVQueryCommandStats queryCommandStats)
            {
                hashes = Hashes;
                queryCommandStats = QueryCommandStats;
            }
        }
    }
}