namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Command;

    public sealed class StatefulRealtimeResultEntryAggregator<T> : IRealtimeAggregator<T> where T : class
    {
        private readonly IRealtimeResultEntryFilter<T> realtimeResultEntryFilter;
        private readonly IRealtimeResultEntryFilter<T> ongoingResultEntryFilter;
        private readonly Action<T> ongoingCallback;
        private readonly ICompletionStrategy<T> completionStrategy;
        private readonly IConcatenator<T> concatenator;
        private readonly ConcurrentDictionary<string, T> trackEntries = new ConcurrentDictionary<string, T>();
        private readonly Func<T, string> getCandidateTrackId;

        public StatefulRealtimeResultEntryAggregator(
            IRealtimeResultEntryFilter<T> realtimeResultEntryFilter, 
            IRealtimeResultEntryFilter<T> ongoingResultEntryFilter,
            Action<T> ongoingCallback,
            ICompletionStrategy<T> completionStrategy,
            IConcatenator<T> concatenator, 
            Func<T, string> getCandidateTrackId)
        {
            this.realtimeResultEntryFilter = realtimeResultEntryFilter;
            this.ongoingResultEntryFilter = ongoingResultEntryFilter;
            this.ongoingCallback = ongoingCallback;
            this.completionStrategy = completionStrategy;
            this.concatenator = concatenator;
            this.getCandidateTrackId = getCandidateTrackId;
        }
        
        /// <inheritdoc cref="IRealtimeAggregator{T}.Consume"/>
        public RealtimeQueryResult<T> Consume(IEnumerable<T>? candidates, double queryLength, double queryOffset)
        {
            var resultEntries = candidates?.ToList() ?? new List<T>();
            SaveNewEntries(resultEntries, queryOffset);
            return PurgeCompleted(resultEntries, queryLength, queryOffset);
        }

        /// <inheritdoc cref="IRealtimeAggregator{T}.Purge"/>
        public RealtimeQueryResult<T> Purge()
        {
            var completed = new List<T>();
            var didNotPassFilter = new HashSet<T>();
            foreach (var resultEntry in trackEntries)
            {
                // we are purging the results hence the match cannot continue in the next query
                if (realtimeResultEntryFilter.Pass(resultEntry.Value, canContinueInTheNextQuery: false))
                {
                    completed.Add(resultEntry.Value);
                }
                else
                {
                    didNotPassFilter.Add(resultEntry.Value);
                }
            }

            return new RealtimeQueryResult<T>(completed, didNotPassFilter);
        }

        private void SaveNewEntries(IEnumerable<T> entries, double queryOffset)
        {
            foreach (var nextEntry in entries)
            {
                trackEntries.AddOrUpdate(getCandidateTrackId(nextEntry), nextEntry, (_, currentEntry) => concatenator.Concat(currentEntry, nextEntry, queryOffset));
            }
        }
        
        private RealtimeQueryResult<T> PurgeCompleted(IEnumerable<T> entries, double queryLength, double queryOffset)
        {
            var set = new HashSet<string>(entries.Select(_ => getCandidateTrackId(_)));
            foreach (var entry in trackEntries.Where(_ => !set.Contains(_.Key)))
            {
                var old = entry.Value;
                var updated = concatenator.WithExtendedQueryLength(old, queryLength + queryOffset);
                trackEntries.TryUpdate(entry.Key, updated, old);
            }
            
            var completed = new List<T>();
            var cantWaitAnymore = new HashSet<T>();
            foreach (KeyValuePair<string, T> pair in trackEntries)
            {
                bool canContinueInNextQuery = completionStrategy.CanContinueInNextQuery(pair.Value);
                if (ongoingResultEntryFilter.Pass(pair.Value, canContinueInNextQuery))
                {
                    // invoke ongoing callback
                    ongoingCallback(pair.Value);
                }
                
                if (!canContinueInNextQuery && trackEntries.TryRemove(pair.Key, out var entry))
                {
                    // can't continue in the next query
                    if (realtimeResultEntryFilter.Pass(entry, canContinueInTheNextQuery: false))
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
                else if (realtimeResultEntryFilter.Pass(pair.Value, canContinueInTheNextQuery: true) && trackEntries.TryRemove(pair.Key, out _))
                {
                    // can continue, but realtime result entry filter takes precedence
                    completed.Add(pair.Value);
                }
            }

            return new RealtimeQueryResult<T>(completed, cantWaitAnymore);
        }
    }
}