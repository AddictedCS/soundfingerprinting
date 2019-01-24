namespace SoundFingerprinting.Query
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;

    public class Matches : IEnumerable<MatchedWith>
    {
        private const float FingerprintLengthInSeconds = 8192 / 5512f;
        
        private readonly SortedList<float, MatchedWith> matches;

        public Matches(IEnumerable<MatchedWith> matches)
        {
            this.matches = new SortedList<float, MatchedWith>();
            foreach (var match in matches)
            {
                if (!this.matches.ContainsKey(match.QueryAt))
                {
                    this.matches.Add(match.QueryAt, match);
                }
            }
        }

        public int EntriesCount => matches.Count;

        public float QueryAtStartsAt => matches.First().Value.QueryAt;
        
        public float TrackAtStartsAt => matches.First().Value.ResultAt;

        public float TotalLength => matches.Last().Key - matches.First().Key + FingerprintLengthInSeconds;

        private float QueryAtEndsAt => matches.Last().Key;

        private float TrackAtEndsAt => matches.Last().Value.ResultAt;

        public IEnumerator<MatchedWith> GetEnumerator()
        {
            return matches.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryCollapseWith(Matches with, double permittedGap, out Matches collapsed)
        {
            Matches current = QueryAtStartsAt <= with.QueryAtStartsAt ? this : with;
            Matches next = current == this ? with : this;

            collapsed = null;
            if (QueryMatchOverlaps(current, next, permittedGap) && TrackMatchOverlaps(current, next, permittedGap))
            {
                collapsed = MergeWith(current, next);
                return true;
            }

            return false;
        }

        private static Matches MergeWith(Matches current, Matches next)
        {
            var concatenated = new List<MatchedWith>();
            MatchedWith[] a = current.ToArray();
            MatchedWith[] b = next.ToArray();

            int ai = 0, bi = 0;
            while (ai < a.Length || bi < b.Length)
            {
                if (ai >= a.Length)
                {
                    concatenated.Add(b[bi++]);
                }
                else if (bi >= b.Length)
                {
                    concatenated.Add(a[ai++]);
                }
                else if (a[ai].QueryAt < b[bi].QueryAt)
                {
                    concatenated.Add(a[ai++]);
                }
                else
                {
                    concatenated.Add(b[bi++]);
                }
            }

            return new Matches(concatenated);
        }

        private static bool QueryMatchOverlaps(Matches current, Matches next, double permittedGap)
        {
            return next.QueryAtStartsAt - current.QueryAtEndsAt <= permittedGap;
        }

        private bool TrackMatchOverlaps(Matches current, Matches next, double permittedGap)
        {
            return current.TrackAtStartsAt <= next.TrackAtEndsAt && next.TrackAtStartsAt - current.TrackAtEndsAt <= permittedGap;
        }
    }
}