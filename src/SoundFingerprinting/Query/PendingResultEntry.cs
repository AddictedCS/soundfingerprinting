namespace SoundFingerprinting.Query
{
    using System;

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
                    collapsed = new PendingResultEntry(Entry.MergeWith(next));
                    return true;
                }
            }

            return false;
        }

        private string InternalUid { get; }
        
        private double TrackMatchEndsAt => Entry.TrackMatchStartsAt + Entry.QueryMatchLength;

        public PendingResultEntry Wait(double length)
        {
            return new PendingResultEntry(new ResultEntry(Entry.Track, Entry.QueryMatchStartsAt, Entry.QueryMatchLength,
                    Entry.QueryCoverageLength, Entry.TrackMatchStartsAt, Entry.TrackStartsAt, Entry.Confidence,
                    Entry.Score, Entry.QueryLength + length, Entry.MatchedAt), waiting + length);
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
            return Entry.TrackMatchStartsAt <= next.TrackMatchStartsAt && TrackMatchEndsAt >= next.TrackMatchStartsAt + next.QueryMatchLength;
        }
    }
}