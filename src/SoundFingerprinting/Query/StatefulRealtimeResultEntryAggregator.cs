namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Command;

    internal sealed class StatefulRealtimeResultEntryAggregator : IRealtimeResultEntryAggregator
    {
        private readonly IRealtimeResultEntryFilter realtimeResultEntryFilter;
        private readonly IRealtimeResultEntryFilter ongoingResultEntryFilter;
        private readonly ICompletionStrategy<AVResultEntry> completionStrategy;
        private readonly IConcatenator<ResultEntry> audioResultEntryConcatenator;
        private readonly IConcatenator<ResultEntry> videoResultEntryConcatenator;

        private readonly IQueryHashesConcatenator queryHashesConcatenator;
        private readonly IOngoingAvResultEntryTracker avResultEntryTracker = new StatefulOngoingAvResultEntryTracker();

        public StatefulRealtimeResultEntryAggregator(
            IRealtimeResultEntryFilter realtimeResultEntryFilter, 
            IRealtimeResultEntryFilter ongoingResultEntryFilter,
            ICompletionStrategy<AVResultEntry> completionStrategy,
            IConcatenator<ResultEntry> audioResultEntryConcatenator,
            IConcatenator<ResultEntry> videoResultEntryConcatenator,
            IQueryHashesConcatenator queryHashesConcatenator)
        {
            this.realtimeResultEntryFilter = realtimeResultEntryFilter;
            this.ongoingResultEntryFilter = ongoingResultEntryFilter;
            this.completionStrategy = completionStrategy;
            this.audioResultEntryConcatenator = audioResultEntryConcatenator;
            this.videoResultEntryConcatenator = videoResultEntryConcatenator;
            this.queryHashesConcatenator = queryHashesConcatenator;
        }
        
        /// <inheritdoc cref="IRealtimeResultEntryAggregator.Consume"/>
        public RealtimeQueryResult Consume(AVQueryResult? queryResult)
        {
            if (queryResult == null)
            {
                return new RealtimeQueryResult([], [], []);
            }
            
            SaveNewEntries(queryResult);
            return PurgeCompleted();
        }

        /// <inheritdoc cref="IRealtimeResultEntryAggregator.Purge"/>
        public RealtimeQueryResult Purge()
        {
            var (completed, didNotPassFilter) = FinalCheckOnEntriesThatCantContinue(avResultEntryTracker.GetAvResultEntries());
            avResultEntryTracker.Clear();
            return GetRealtimeQueryResult([], completed, didNotPassFilter);
        }
        
        private void SaveNewEntries(AVQueryResult queryResult)
        {
            var (audioHashes, videoHashes) = queryResult.QueryHashes;
            double audioQueryOffset = audioHashes?.TimeOffset ?? 0;
            double videoQueryOffset = videoHashes?.TimeOffset ?? 0;
            
            // getting one best result entry per track, as all following code assumes there is only one entry per track
            var newEntries = GetBestMatchPerTrack(queryResult);
            foreach (var next in newEntries)
            {
                var (nextAudio, nextVideo) = next;
                avResultEntryTracker.AddOrUpdate(next, prev =>
                {
                    var (prevAudio, prevVideo) = prev;
                    var audio = audioResultEntryConcatenator.Concat(prevAudio, nextAudio, audioQueryOffset);
                    var video = videoResultEntryConcatenator.Concat(prevVideo, nextVideo, videoQueryOffset);
                    return new AVResultEntry(audio, video);
                });
            }
            
            // we need to extend the query length of those matches that haven't been purged on the previous call, and haven't been updated during this call.
            double audioQueryLength = audioHashes?.DurationInSeconds ?? 0;
            double videoQueryLength = videoHashes?.DurationInSeconds ?? 0;
            foreach (var notUpdated in avResultEntryTracker.GetAvResultEntriesExcept(newEntries))
            {
                var (prevAudio, prevVideo) = notUpdated;
                var audio = ExtendResultEntryQueryLength(prevAudio, audioQueryLength, audioQueryOffset);
                var video = ExtendResultEntryQueryLength(prevVideo, videoQueryLength, videoQueryOffset);
                avResultEntryTracker.AddOrUpdate(notUpdated, _ => new AVResultEntry(audio, video));
            }
            
            queryHashesConcatenator.UpdateHashesForTracks(avResultEntryTracker.GetTrackIds(), queryResult.QueryHashes, queryResult.QueryCommandStats);
        }

        private static List<AVResultEntry> GetBestMatchPerTrack(AVQueryResult queryResult)
        {
            return queryResult.ResultEntries
                .GroupBy(_ => _.TrackId)
                .Select(gr => gr.OrderByDescending(avResultEntry =>
                    {
                        var (audio, video) = avResultEntry;
                        return (audio?.Coverage.TrackCoverageWithPermittedGapsLength ?? 0) + (video?.Coverage.TrackCoverageWithPermittedGapsLength ?? 0);
                    })
                    .First())
                .ToList();
        }

        private static ResultEntry? ExtendResultEntryQueryLength(ResultEntry? notUpdated, double queryLength, double queryOffset)
        {
            double extendedBy = queryLength + queryOffset;
            return notUpdated != null ? new ResultEntry(notUpdated.Track, notUpdated.Score, notUpdated.MatchedAt, notUpdated.Coverage.WithExtendedQueryLength(extendedBy)) : null;
        }

        private RealtimeQueryResult PurgeCompleted()
        {
            var ongoing = new List<AVResultEntry>();
            var completed = new List<AVResultEntry>();
            var didNotPassFilter = new List<AVResultEntry>();
            foreach (var avResultEntry in avResultEntryTracker.GetAvResultEntries())
            {
                bool canContinueInNextQuery = completionStrategy.CanContinueInNextQuery(avResultEntry);
                if (ongoingResultEntryFilter.Pass(avResultEntry, canContinueInNextQuery))
                {
                    // ongoing filter
                    ongoing.Add(avResultEntry);
                }
                
                // track is removed either because it can't continue in the next query, or because it passed the filter
                if (!canContinueInNextQuery && avResultEntryTracker.TryRemove(avResultEntry))
                {
                    // can't continue in the next query
                    if (realtimeResultEntryFilter.Pass(avResultEntry, canContinueInTheNextQuery: false))
                    {
                        // passed entry filter
                        completed.Add(avResultEntry);
                    }
                    else
                    {
                        // did not pass filter
                        didNotPassFilter.Add(avResultEntry);
                    }
                }
                else if (realtimeResultEntryFilter.Pass(avResultEntry, canContinueInTheNextQuery: true) && avResultEntryTracker.TryRemove(avResultEntry))
                {
                    // can continue, but realtime result entry filter takes precedence
                    completed.Add(avResultEntry);
                }
            }
            
            var (timeShiftCompleted, timeShiftedDidNotPassFilterEntries) = FinalCheckOnEntriesThatCantContinue(avResultEntryTracker.GetAndRemoveTimeShiftedEntries());
            return GetRealtimeQueryResult(ongoing, completed.Concat(timeShiftCompleted), didNotPassFilter.Concat(timeShiftedDidNotPassFilterEntries));
        }

        private RealtimeQueryResult GetRealtimeQueryResult(IEnumerable<AVResultEntry> ongoing, IEnumerable<AVResultEntry> completed, IEnumerable<AVResultEntry> didNotPassFilter)
        {
            var completedEntries = completed as AVResultEntry[] ?? completed.ToArray();
            var completedGroups =  queryHashesConcatenator.GetQueryResults(completedEntries);
            var didNotPassFilterEntries = didNotPassFilter as AVResultEntry[] ?? didNotPassFilter.ToArray();
            var didNotPassGroups = queryHashesConcatenator.GetQueryResults(didNotPassFilterEntries);
            var notTrackedTracks = completedEntries.Concat(didNotPassFilterEntries).Select(_ => _.TrackId).Except(avResultEntryTracker.GetTrackIds());
            queryHashesConcatenator.Cleanup(notTrackedTracks);
            return new RealtimeQueryResult(ongoing, completedGroups, didNotPassGroups);
        }
        
        private Tuple<IEnumerable<AVResultEntry>, IEnumerable<AVResultEntry>> FinalCheckOnEntriesThatCantContinue(IEnumerable<AVResultEntry> avResultEntries)
        {
            var completed = new List<AVResultEntry>();
            var didNotPassFilter = new List<AVResultEntry>();
            foreach (var resultEntry in avResultEntries)
            {
                if (realtimeResultEntryFilter.Pass(resultEntry, canContinueInTheNextQuery: false))
                {
                    completed.Add(resultEntry);
                }
                else
                {
                    didNotPassFilter.Add(resultEntry);
                }
            }

            return new Tuple<IEnumerable<AVResultEntry>, IEnumerable<AVResultEntry>>(completed, didNotPassFilter);
        }
    }
}