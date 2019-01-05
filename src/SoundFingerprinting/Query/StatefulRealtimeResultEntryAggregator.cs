namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Command;

    public class StatefulRealtimeResultEntryAggregator : IRealtimeResultEntryAggregator
    {
        private readonly object lockObject = new object();
        private List<PendingResultEntry> pendingResults = new List<PendingResultEntry>();
        
        public RealtimeQueryResult
            Consume(IEnumerable<ResultEntry> candidates, 
            IRealtimeResultEntryFilter realtimeResultEntryFilter, 
            double queryLength)
        {
            lock (lockObject)
            {
                CollapseWithNewArrivals(candidates);
                IncreaseWaitTime(queryLength);
                return PurgeCompletedMatches(realtimeResultEntryFilter);
            }
        }

        private RealtimeQueryResult PurgeCompletedMatches(IRealtimeResultEntryFilter resultEntryFilter)
        {
            var completed = new HashSet<PendingResultEntry>();
            var cantWaitAnymore = new HashSet<PendingResultEntry>();
            
            foreach (var entry in pendingResults)
            {
                if (resultEntryFilter.Pass(entry.Entry))
                {
                    completed.Add(entry);
                }
                else if (!entry.CanWait)
                {
                    cantWaitAnymore.Add(entry);
                }
            }

            pendingResults = pendingResults.Where(match => !completed.Contains(match) && !cantWaitAnymore.Contains(match))
                                           .ToList();
            
            return new RealtimeQueryResult(completed.Select(entry => entry.Entry).ToList(), cantWaitAnymore.Select(entry => entry.Entry).ToList());
        }

        private void IncreaseWaitTime(double queryLength)
        {
            foreach (var pendingResult in pendingResults)
            {
                pendingResult.Wait(queryLength);
            }
        }

        private void CollapseWithNewArrivals(IEnumerable<ResultEntry> candidates)
        {
            var newArrivals = candidates.Select(candidate => new PendingResultEntry(candidate)).ToList();

            var cartesian = from newArrival in newArrivals
                from pending in pendingResults
                select new {NewArrival = newArrival, Pending = pending};

            var accumulator = new
            {
                AlreadyCollapsed = new HashSet<PendingResultEntry>(),
                NewPendingMatches = new List<PendingResultEntry>(),
            };

            var result = cartesian.Aggregate(accumulator, (current, pair) =>
            {
                var alreadyCollapsed = current.AlreadyCollapsed;
                if (alreadyCollapsed.Contains(pair.NewArrival) || alreadyCollapsed.Contains(pair.Pending))
                {
                    return current;
                }

                if (pair.Pending.TryCollapse(pair.NewArrival, out var collapsed))
                {
                    alreadyCollapsed.Add(pair.Pending);
                    alreadyCollapsed.Add(pair.NewArrival);
                    current.NewPendingMatches.Add(collapsed);
                }

                return current;
            });

            pendingResults = pendingResults.Where(match => !result.AlreadyCollapsed.Contains(match))
                                           .Concat(result.NewPendingMatches)
                                           .Concat(newArrivals.Where(match => !result.AlreadyCollapsed.Contains(match))) 
                                           .ToList();
        }
    }
}