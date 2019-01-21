namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class Matches : IEnumerable<MatchedWith>
    {
        private readonly List<MatchedWith> matches;

        public Matches(List<MatchedWith> matches)
        {
            this.matches = matches;
        }

        public double QueryAtStartsAt
        {
            get { throw new NotImplementedException(); }
        }

        public double TrackAtStartAt
        {
            get { throw new NotImplementedException(); }
        }

        public double TotalLength
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator<MatchedWith> GetEnumerator()
        {
            return matches.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Matches MergeWith(Matches with)
        {
            throw new NotImplementedException();
        }

        public bool TryCollapseWith(Matches with, double permittedGap, out Matches collapsed)
        {
            throw new NotImplementedException();
        }
    }
}