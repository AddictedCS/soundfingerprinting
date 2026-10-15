namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using SoundFingerprinting.Command;

    internal sealed class StatefulRealtimeResultEntryAggregator : IRealtimeResultEntryAggregator
    {
        // re-deriving gaps over a stitched best path can shave fractions of a second; a merge that loses
        // more than this from an entry that claims the same match is a defect, not arithmetic noise
        private const double CoverageShrinkToleranceSeconds = 0.5;

        private readonly IRealtimeResultEntryFilter realtimeResultEntryFilter;
        private readonly IRealtimeResultEntryFilter ongoingResultEntryFilter;
        private readonly ICompletionStrategy<AVResultEntry> completionStrategy;
        private readonly IConcatenator<ResultEntry> audioResultEntryConcatenator;
        private readonly IConcatenator<ResultEntry> videoResultEntryConcatenator;

        private readonly IQueryHashesConcatenator queryHashesConcatenator;
        private readonly IOngoingAvResultEntryTracker avResultEntryTracker = new StatefulOngoingAvResultEntryTracker();
        private readonly ILogger<StatefulRealtimeResultEntryAggregator> logger;

        public StatefulRealtimeResultEntryAggregator(
            IRealtimeResultEntryFilter realtimeResultEntryFilter, 
            IRealtimeResultEntryFilter ongoingResultEntryFilter,
            ICompletionStrategy<AVResultEntry> completionStrategy,
            IConcatenator<ResultEntry> audioResultEntryConcatenator,
            IConcatenator<ResultEntry> videoResultEntryConcatenator,
            IQueryHashesConcatenator queryHashesConcatenator,
            ILoggerFactory loggerFactory)
        {
            this.realtimeResultEntryFilter = realtimeResultEntryFilter;
            this.ongoingResultEntryFilter = ongoingResultEntryFilter;
            this.completionStrategy = completionStrategy;
            this.audioResultEntryConcatenator = audioResultEntryConcatenator;
            this.videoResultEntryConcatenator = videoResultEntryConcatenator;
            this.queryHashesConcatenator = queryHashesConcatenator;
            logger = loggerFactory.CreateLogger<StatefulRealtimeResultEntryAggregator>();
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
            double audioQueryLength = audioHashes?.DurationInSeconds ?? 0;
            double videoQueryLength = videoHashes?.DurationInSeconds ?? 0;
            
            // one entry per track, as all following code assumes there is only one entry per track; a track that repeats its audio
            // yields one candidate per copy, and the candidate that continues the tracked occurrence wins over the one with more coverage
            var candidatesPerTrack = queryResult.ResultEntries
                .GroupBy(_ => _.TrackId)
                .ToDictionary(gr => gr.Key, gr => (IReadOnlyList<AVResultEntry>)gr.OrderByDescending(GetSummedTrackCoverage).ToList());
            var newEntries = candidatesPerTrack.Values.Select(candidates => candidates[0]).ToList();
            foreach (var next in newEntries)
            {
                avResultEntryTracker.AddOrUpdate(next, prev =>
                {
                    var (prevAudio, prevVideo) = prev;
                    var merged = SelectContinuation(prev, candidatesPerTrack[next.TrackId], audioQueryOffset, videoQueryOffset);
                    if (merged.IsEquivalent(prev) && GetSummedTrackCoverage(merged) < GetSummedTrackCoverage(prev) - CoverageShrinkToleranceSeconds)
                    {
                        // a merge that claims the same match must never lose accumulated coverage; keep the
                        // previous entry (query length extended) so the completion filter judges the peak value
                        logger.LogWarning(
                            "Merging track {TrackId} matched at {MatchedAt:O} shrank accumulated coverage from {Previous:0.00}s to {Merged:0.00}s, keeping the accumulated entry",
                            prev.TrackId,
                            prev.MatchedAt,
                            GetSummedTrackCoverage(prev),
                            GetSummedTrackCoverage(merged));
                        var keptAudio = ExtendResultEntryQueryLength(prevAudio, audioQueryLength, audioQueryOffset);
                        var keptVideo = ExtendResultEntryQueryLength(prevVideo, videoQueryLength, videoQueryOffset);
                        return new AVResultEntry(keptAudio, keptVideo);
                    }

                    return merged;
                });
            }
            
            // we need to extend the query length of those matches that haven't been purged on the previous call, and haven't been updated during this call.
            foreach (var notUpdated in avResultEntryTracker.GetAvResultEntriesExcept(newEntries))
            {
                var (prevAudio, prevVideo) = notUpdated;
                var audio = ExtendResultEntryQueryLength(prevAudio, audioQueryLength, audioQueryOffset);
                var video = ExtendResultEntryQueryLength(prevVideo, videoQueryLength, videoQueryOffset);
                avResultEntryTracker.AddOrUpdate(notUpdated, _ => new AVResultEntry(audio, video));
            }
            
            queryHashesConcatenator.UpdateHashesForTracks(avResultEntryTracker.GetTrackIds(), queryResult.QueryHashes, queryResult.QueryCommandStats);
        }

        /// <summary>
        ///  Merges the first candidate, closest to the diagonal first, that continues the tracked entry; when none continues, the merge of the
        ///  best candidate is returned, which the concatenator hands back untouched so the tracker starts a new occurrence.
        /// </summary>
        /// <param name="prev">Tracked entry.</param>
        /// <param name="candidates">Candidates for the tracked entry's track, best coverage first.</param>
        /// <param name="audioQueryOffset">Audio query offset.</param>
        /// <param name="videoQueryOffset">Video query offset.</param>
        /// <returns>Merged entry.</returns>
        private AVResultEntry SelectContinuation(AVResultEntry prev, IReadOnlyList<AVResultEntry> candidates, double audioQueryOffset, double videoQueryOffset)
        {
            var (prevAudio, prevVideo) = prev;
            AVResultEntry? fallback = null;
            // the candidate whose track advance matches the stream advance is the continuation; a copy of the audio elsewhere in the track has more drift
            double permittedGap = (prevAudio ?? prevVideo)!.Coverage.PermittedGap;
            foreach (var candidate in candidates.OrderBy(candidate => Math.Max(0, SeamDrift(prev, candidate) - permittedGap)).ThenByDescending(GetSummedTrackCoverage))
            {
                var (nextAudio, nextVideo) = candidate;
                var audio = audioResultEntryConcatenator.Concat(prevAudio, nextAudio, audioQueryOffset);
                var video = videoResultEntryConcatenator.Concat(prevVideo, nextVideo, videoQueryOffset);
                var merged = new AVResultEntry(audio, video);
                // audio decides when both entries carry it, otherwise video; the concatenator returns the incoming entry itself when it is not a continuation
                bool continues = prevAudio != null && nextAudio != null
                    ? !ReferenceEquals(audio, nextAudio)
                    : prevVideo != null && nextVideo != null && !ReferenceEquals(video, nextVideo);
                if (continues)
                {
                    return merged;
                }

                fallback ??= ReferenceEquals(candidate, candidates[0]) ? merged : null;
            }

            return fallback ?? SelectContinuationFallback(prev, candidates[0], audioQueryOffset, videoQueryOffset);
        }

        private AVResultEntry SelectContinuationFallback(AVResultEntry prev, AVResultEntry best, double audioQueryOffset, double videoQueryOffset)
        {
            var (prevAudio, prevVideo) = prev;
            var (nextAudio, nextVideo) = best;
            return new AVResultEntry(audioResultEntryConcatenator.Concat(prevAudio, nextAudio, audioQueryOffset), videoResultEntryConcatenator.Concat(prevVideo, nextVideo, videoQueryOffset));
        }

        private static double SeamDrift(AVResultEntry prev, AVResultEntry candidate)
        {
            var previous = (prev.Audio ?? prev.Video)!;
            var next = (candidate.Audio ?? candidate.Video)!;
            double trackAdvance = next.TrackMatchStartsAt - previous.TrackMatchStartsAt;
            double streamAdvance = (candidate.MatchedAt - prev.MatchedAt).TotalSeconds;
            return Math.Abs(trackAdvance - streamAdvance);
        }

        private static double GetSummedTrackCoverage(AVResultEntry entry)
        {
            var (audio, video) = entry;
            return (audio?.Coverage.TrackCoverageWithPermittedGapsLength ?? 0) + (video?.Coverage.TrackCoverageWithPermittedGapsLength ?? 0);
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