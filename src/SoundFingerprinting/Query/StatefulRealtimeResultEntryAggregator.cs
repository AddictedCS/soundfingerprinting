namespace SoundFingerprinting.Query
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.LCS;

    public sealed class StatefulRealtimeResultEntryAggregator : IRealtimeResultEntryAggregator
    {
        private readonly IRealtimeResultEntryFilter realtimeResultEntryFilter;
        private readonly ICompletionStrategy<ResultEntry> completionStrategy;
        private readonly IResultEntryConcatenator concatenator;
        private readonly ConcurrentDictionary<string, ResultEntry> trackEntries = new ConcurrentDictionary<string, ResultEntry>();

        public StatefulRealtimeResultEntryAggregator(IRealtimeResultEntryFilter realtimeResultEntryFilter, QueryConfiguration queryConfiguration)
        {
            this.realtimeResultEntryFilter = realtimeResultEntryFilter;
            completionStrategy = new ResultEntryCompletionStrategy(queryConfiguration.PermittedGap);
            concatenator = new ResultEntryConcatenator(queryConfiguration);
        }
        
        public RealtimeQueryResult Consume(IEnumerable<ResultEntry>? candidates, double queryLength, double queryOffset)
        {
            var resultEntries = candidates?.ToList() ?? new List<ResultEntry>();
            SaveNewEntries(resultEntries, queryOffset);
            return PurgeCompleted(resultEntries, queryLength, queryOffset);
        }
        
        private void SaveNewEntries(IEnumerable<ResultEntry> entries, double queryOffset)
        {
            foreach (var nextEntry in entries)
            {
                trackEntries.AddOrUpdate(nextEntry.Track.Id, nextEntry, (_, currentEntry) => concatenator.Concat(currentEntry, nextEntry, queryOffset));
            }
        }
        
        private RealtimeQueryResult PurgeCompleted(IEnumerable<ResultEntry> entries, double queryLength, double queryOffset)
        {
            var set = new HashSet<string>(entries.Select(_ => _.Track.Id));
            foreach (var entry in trackEntries.Where(_ => !set.Contains(_.Key)))
            {
                var old = entry.Value;
                var updated = new ResultEntry(old.Track, old.Score, old.MatchedAt, new Coverage(old.Coverage.BestPath, old.Coverage.QueryLength + queryLength + queryOffset, old.Coverage.TrackLength, old.Coverage.FingerprintLength, old.Coverage.PermittedGap));
                trackEntries.TryUpdate(entry.Key, updated, old);
            }
            
            var completed = new List<ResultEntry>();
            var cantWaitAnymore = new HashSet<ResultEntry>();
            foreach (KeyValuePair<string, ResultEntry> pair in trackEntries)
            {
                if (!completionStrategy.CanContinueInNextQuery(pair.Value) && trackEntries.TryRemove(pair.Key, out var entry))
                {
                    // can't continue in the next query
                    if (realtimeResultEntryFilter.Pass(entry))
                    {
                        // passed entry filter
                        completed.Add(entry);
                    }
                    else
                    {
                        // did not pass filter
                        cantWaitAnymore.Add(entry);
                    }
                }
                else if (realtimeResultEntryFilter.Pass(pair.Value) && trackEntries.TryRemove(pair.Key, out _))
                {
                    // can continue, but realtime result entry filter takes precedence
                    completed.Add(pair.Value);
                }
            }

            return new RealtimeQueryResult(Sorted(completed), Sorted(cantWaitAnymore));
        }

        private static IEnumerable<ResultEntry> Sorted(IEnumerable<ResultEntry> entries)
        {
            return entries
                .OrderByDescending(e => e.Confidence)
                .ThenBy(e => e.Track.Id)
                .ToList();
        }
    }
}