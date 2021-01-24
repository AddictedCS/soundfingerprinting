namespace SoundFingerprinting.Query
{
    using System;
    using System.Linq;
    using SoundFingerprinting.LCS;

    public class PendingResultEntry
    {
        private readonly double waiting;

        public PendingResultEntry(ResultEntry entry, double waiting = 0)
        {
            Entry = entry;
            InternalUid = Guid.NewGuid().ToString();
            this.waiting = waiting;
        }

        public ResultEntry Entry { get; }

        public bool TryCollapse(PendingResultEntry pendingNext, double permittedGap, out PendingResultEntry collapsed)
        {
            var next = pendingNext.Entry;
            collapsed = null;
            if (Entry.Track.Equals(next.Track))
            {
                if (TrackMatchOverlaps(next, permittedGap) || CanSwallow(next))
                {
                    collapsed = new PendingResultEntry(MergeWith(Entry, next));
                    return true;
                }
            }

            return false;
        }

        private string InternalUid { get; }
        
        private double TrackMatchEndsAt => Entry.TrackMatchStartsAt + Entry.TrackCoverageWithPermittedGapsLength;

        public PendingResultEntry Wait(double length)
        {
            var newCoverage = new Coverage(Entry.Coverage.BestPath, Entry.QueryLength + length, Entry.Coverage.TrackLength, Entry.Coverage.FingerprintLength, Entry.Coverage.PermittedGap);
            var resultEntry = new ResultEntry(Entry.Track, Entry.Score, Entry.MatchedAt, newCoverage);
            return new PendingResultEntry(resultEntry, waiting + length);
        }

        public bool CanWait(double accuracyDelta)
        {
            return waiting < accuracyDelta;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PendingResultEntry item))
            {
                return false;
            }
            
            return InternalUid == item.InternalUid;
        }

        public override int GetHashCode()
        {
            return InternalUid != null ? InternalUid.GetHashCode() : 0;
        }
        
        private bool TrackMatchOverlaps(ResultEntry next, double permittedGap)
        {
            return TrackMatchEndsAt >= next.TrackMatchStartsAt - permittedGap && next.TrackMatchStartsAt + permittedGap >= TrackMatchEndsAt;
        }

        private bool CanSwallow(ResultEntry next)
        {
            return Entry.TrackMatchStartsAt <= next.TrackMatchStartsAt && TrackMatchEndsAt >= next.TrackMatchStartsAt + next.TrackCoverageWithPermittedGapsLength;
        }
        
        private static ResultEntry MergeWith(ResultEntry entry, ResultEntry with)
        {
            if (!entry.Track.Equals(with.Track))
            {
                throw new ArgumentException($"{nameof(with)} merging entries should correspond to the same track");
            }

            var mergedCoverage = MergeWith(entry.Coverage, with.Coverage);
            var matchedAt = entry.MatchedAt < with.MatchedAt ? entry.MatchedAt : with.MatchedAt;
            return new ResultEntry(entry.Track, entry.Score + with.Score, matchedAt, mergedCoverage);
        }
        
        private static Coverage MergeWith(Coverage first, Coverage second)
        {
            if (first.Contains(second))
            {
                return first;
            }

            if (second.Contains(first))
            {
                return second;
            }

            // this is not exactly right but let's leave it as is at the moment
            var bestPath = first.BestPath
                .Concat(second.BestPath)
                .OrderBy(_ => _.TrackMatchAt)
                .Select((matched, index) => new MatchedWith(matched.TrackSequenceNumber, matched.TrackMatchAt, matched.TrackSequenceNumber, matched.TrackMatchAt, matched.Score));
            return new Coverage(bestPath, CalculateNewQueryLength(first, second), first.TrackLength, first.FingerprintLength, first.PermittedGap);
        }

        private static double CalculateNewQueryLength(Coverage a, Coverage b)
        {
            if (Math.Abs(a.TrackMatchStartsAt - b.TrackMatchStartsAt) < 0.0001)
            {
                // same start
                return a.QueryLength;
            }

            // t --------------
            // a      ---------
            // b   --------
            if (a.TrackMatchStartsAt > b.TrackMatchStartsAt)
            {
                double diff = a.TrackMatchStartsAt - b.TrackMatchStartsAt;
                if (diff > b.TrackMatchStartsAt)
                {
                    return a.QueryLength + b.QueryLength;
                }

                return diff + a.QueryLength;
            }
            else
            {
                // t -------------
                // a  ------
                // b      -----
                double diff = b.TrackMatchStartsAt - a.TrackMatchStartsAt;
                if (diff > a.QueryLength)
                {
                    return a.QueryLength + b.QueryLength;
                }

                return diff + b.QueryLength;
            }
        }
    }
}