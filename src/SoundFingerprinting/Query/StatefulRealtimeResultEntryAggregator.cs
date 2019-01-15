namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Command;

    public class StatefulRealtimeResultEntryAggregator : IRealtimeResultEntryAggregator
    {
        private readonly IRealtimeResultEntryFilter realtimeResultEntryFilter;
        private readonly double permittedGap;
        private readonly object lockObject = new object();
        
        private List<PendingResultEntry> pendingResults = new List<PendingResultEntry>();

        public StatefulRealtimeResultEntryAggregator(IRealtimeResultEntryFilter realtimeResultEntryFilter, double permittedGap)
        {
            this.realtimeResultEntryFilter = realtimeResultEntryFilter;
            this.permittedGap = permittedGap;
        }
        
        public RealtimeQueryResult Consume(IEnumerable<ResultEntry> candidates, double queryLength)
        {
            lock (lockObject)
            {
                var collapsed = CollapseWithNewArrivals(pendingResults, candidates ?? Enumerable.Empty<ResultEntry>(), queryLength, permittedGap);
                return PurgeCompletedMatches(collapsed, realtimeResultEntryFilter, permittedGap, out pendingResults);
            }
        }

        private static RealtimeQueryResult PurgeCompletedMatches(
            IEnumerable<PendingResultEntry> pendingResults, 
            IRealtimeResultEntryFilter realtimeResultEntryFilter,
            double permittedGap, out List<PendingResultEntry> leftPendingResultEntries)
        {
            var resultEntries = pendingResults.ToList();
            if (!resultEntries.Any())
            {
                leftPendingResultEntries = new List<PendingResultEntry>();
                return new RealtimeQueryResult(Enumerable.Empty<ResultEntry>(), Enumerable.Empty<ResultEntry>());
            }
            
            var completed = new HashSet<PendingResultEntry>();
            var cantWaitAnymore = new HashSet<PendingResultEntry>();
            foreach (var entry in resultEntries)
            {
                if (realtimeResultEntryFilter.Pass(entry.Entry))
                {
                    completed.Add(entry);
                }
                else if (!entry.CanWait(permittedGap))
                {
                    cantWaitAnymore.Add(entry);
                }
            }

            leftPendingResultEntries = resultEntries.Where(match => !completed.Contains(match) && !cantWaitAnymore.Contains(match))
                                                    .ToList();
            
            return new RealtimeQueryResult(completed.Select(entry => entry.Entry).ToList(), cantWaitAnymore.Select(entry => entry.Entry).ToList());
        }

        private static IEnumerable<PendingResultEntry> CollapseWithNewArrivals(
            IEnumerable<PendingResultEntry> oldResultEntries, 
            IEnumerable<ResultEntry> newResultEntries, 
            double queryLength, 
            double permittedGap)
        {
            var newArrivals = newResultEntries.Select(candidate => new PendingResultEntry(candidate))
                                              .ToList();

            var cartesian = from newArrival in newArrivals
                from pending in oldResultEntries
                select new {NewArrival = newArrival, Pending = pending};

            var accumulator = new
            {
                AlreadyCollapsed = new HashSet<PendingResultEntry>(),
                NewPendingMatches = new List<PendingResultEntry>(),
            };

            var cartesianAggregation = cartesian.Aggregate(accumulator, (current, pair) =>
            {
                var alreadyCollapsed = current.AlreadyCollapsed;
                if (alreadyCollapsed.Contains(pair.NewArrival) || alreadyCollapsed.Contains(pair.Pending))
                {
                    return current;
                }

                if (pair.Pending.TryCollapse(pair.NewArrival, permittedGap, out var collapsed))
                {
                    alreadyCollapsed.Add(pair.Pending);
                    alreadyCollapsed.Add(pair.NewArrival);
                    current.NewPendingMatches.Add(collapsed);
                }

                return current;
            });

            return oldResultEntries.Where(match => !cartesianAggregation.AlreadyCollapsed.Contains(match))
                                   .Select(old => old.Wait(queryLength)) 
                                   .Concat(cartesianAggregation.NewPendingMatches)
                                   .Concat(newArrivals.Where(match => !cartesianAggregation.AlreadyCollapsed.Contains(match))) 
                                   .ToList();
        }
    }
}