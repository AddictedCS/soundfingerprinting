namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IRealtimeResultEntryAggregator
    {
        IEnumerable<ResultEntry> Consume(IEnumerable<ResultEntry> candidates, double secondsThreshold, double queryLength);
    }
    
    public class StatefulRealtimeResultEntryAggregator : IRealtimeResultEntryAggregator
    {
        private readonly object lockObject = new object();
        private List<PendingResultEntry> pendingResults = new List<PendingResultEntry>();
        
        public IEnumerable<ResultEntry> Consume(IEnumerable<ResultEntry> candidates, double secondsThreshold, double queryLength)
        {
            lock (lockObject)
            {
                CollapseWithNewArrivals(candidates);
                IncreaseWaitTime(queryLength);
                return PurgeCompletedMatches(secondsThreshold);
            }
        }

        private IEnumerable<ResultEntry> PurgeCompletedMatches(double secondsThreshold)
        {
            var completed = new HashSet<PendingResultEntry>();
            foreach (var entry in pendingResults)
            {
                if (entry.IsCompleted(secondsThreshold))
                {
                    completed.Add(entry);
                }
            }

            pendingResults = pendingResults.Where(match => !completed.Contains(match))
                                           .ToList();
            return completed.Select(pending => pending.Entry);
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