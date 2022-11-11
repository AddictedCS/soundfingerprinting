namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Command;

    internal sealed class StatefulRealtimeResultEntryAggregator : IRealtimeAggregator
    {
        private readonly IRealtimeResultEntryFilter realtimeResultEntryFilter;
        private readonly IRealtimeResultEntryFilter ongoingResultEntryFilter;
        private readonly Action<AVResultEntry> ongoingCallback;
        private readonly ICompletionStrategy<AVResultEntry> completionStrategy;
        private readonly IConcatenator<ResultEntry> audioResultEntryConcatenator;
        private readonly IConcatenator<ResultEntry> videoResultEntryConcatenator;

        private readonly IQueryHashesConcatenator queryHashesConcatenator;
        private readonly ConcurrentDictionary<string, AVResultEntry> trackEntries = new ();

        public StatefulRealtimeResultEntryAggregator(
            IRealtimeResultEntryFilter realtimeResultEntryFilter, 
            IRealtimeResultEntryFilter ongoingResultEntryFilter,
            Action<AVResultEntry> ongoingCallback,
            ICompletionStrategy<AVResultEntry> completionStrategy,
            IConcatenator<ResultEntry> audioResultEntryConcatenator,
            IConcatenator<ResultEntry> videoResultEntryConcatenator,
            IQueryHashesConcatenator queryHashesConcatenator)
        {
            this.realtimeResultEntryFilter = realtimeResultEntryFilter;
            this.ongoingResultEntryFilter = ongoingResultEntryFilter;
            this.ongoingCallback = ongoingCallback;
            this.completionStrategy = completionStrategy;
            this.audioResultEntryConcatenator = audioResultEntryConcatenator;
            this.videoResultEntryConcatenator = videoResultEntryConcatenator;
            this.queryHashesConcatenator = queryHashesConcatenator;
        }
        
        /// <inheritdoc cref="IRealtimeAggregator.Consume"/>
        public RealtimeQueryResult Consume(AVQueryResult? queryResult)
        {
            if (queryResult == null)
            {
                return new RealtimeQueryResult(Enumerable.Empty<AVQueryResult>(), Enumerable.Empty<AVQueryResult>());
            }
            
            SaveNewEntries(queryResult);
            return PurgeCompleted();
        }

        /// <inheritdoc cref="IRealtimeAggregator.Purge"/>
        public RealtimeQueryResult Purge()
        {
            var completed = new List<AVResultEntry>();
            var didNotPassFilter = new HashSet<AVResultEntry>();
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

            return GetRealtimeQueryResult(completed, didNotPassFilter);
        }

        private void SaveNewEntries(AVQueryResult queryResult)
        {
            var (audioHashes, videoHashes) = queryResult.QueryHashes;
            double audioQueryOffset = audioHashes?.TimeOffset ?? 0;
            double videoQueryOffset = videoHashes?.TimeOffset ?? 0;
            
            var newEntries = queryResult.ResultEntries.ToList();
            foreach (var next in newEntries)
            {
                var (nextAudio, nextVideo) = next;
                trackEntries.AddOrUpdate(next.TrackId, next, (_, prev) =>
                {
                    var (prevAudio, prevVideo) = prev;
                    var audio = audioResultEntryConcatenator.Concat(prevAudio, nextAudio, audioQueryOffset);
                    var video = videoResultEntryConcatenator.Concat(prevVideo, nextVideo, videoQueryOffset);
                    return new AVResultEntry(audio, video);
                });
            }
            
            // we need to extend the query length of those matches that haven't been purged on the previous call, and haven't been updated during this call.
            var set = new HashSet<string>(newEntries.Select(_ => _.TrackId));
            double audioQueryLength = audioHashes?.DurationInSeconds ?? 0;
            double videoQueryLength = videoHashes?.DurationInSeconds ?? 0;
            foreach (var notUpdatedPair in trackEntries.Where(_ => !set.Contains(_.Key)))
            {
                var notUpdated = notUpdatedPair.Value;
                var audio = ExtendResultEntryQueryLength(notUpdated.Audio, audioQueryLength, audioQueryOffset);
                var video = ExtendResultEntryQueryLength(notUpdated.Video, videoQueryLength, videoQueryOffset);
                trackEntries.TryUpdate(notUpdatedPair.Key, new AVResultEntry(audio, video), notUpdated);
            }
            
            queryHashesConcatenator.UpdateHashesForTracks(trackEntries.Keys, queryResult.QueryHashes, queryResult.QueryCommandStats);
        }

        private static ResultEntry? ExtendResultEntryQueryLength(ResultEntry? notUpdated, double queryLength, double queryOffset)
        {
            double extendedBy = queryLength + queryOffset;
            return notUpdated != null ? new ResultEntry(notUpdated.Track, notUpdated.Score, notUpdated.MatchedAt, notUpdated.Coverage.WithExtendedQueryLength(extendedBy)) : null;
        }

        private RealtimeQueryResult PurgeCompleted()
        {
            var completed = new List<AVResultEntry>();
            var didNotPassFilter = new HashSet<AVResultEntry>();
            foreach (KeyValuePair<string, AVResultEntry> pair in trackEntries)
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
                        didNotPassFilter.Add(entry);
                    }
                }
                else if (realtimeResultEntryFilter.Pass(pair.Value, canContinueInTheNextQuery: true) && trackEntries.TryRemove(pair.Key, out _))
                {
                    // can continue, but realtime result entry filter takes precedence
                    completed.Add(pair.Value);
                }
            }

            return GetRealtimeQueryResult(completed, didNotPassFilter);
        }

        private RealtimeQueryResult GetRealtimeQueryResult(IReadOnlyCollection<AVResultEntry> completed, HashSet<AVResultEntry> didNotPassFilter)
        {
            var completedGroups =  queryHashesConcatenator.GetQueryResults(completed);
            var didNotPassGroups = queryHashesConcatenator.GetQueryResults(didNotPassFilter);
            queryHashesConcatenator.Cleanup(completed.Concat(didNotPassFilter).Select(_ => _.TrackId));
            return new RealtimeQueryResult(completedGroups, didNotPassGroups);
        }
    }
}