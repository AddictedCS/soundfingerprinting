using System.Collections.Generic;
namespace SoundFingerprinting.Query
{
    using System;
    using System.Linq;

    public static class Extensions
    {
        public static double StdDev(this IEnumerable<double> values)
        {
            double ret = 0;
            var doubles = values.ToList();
            int count = doubles.Count();
            if (count > 1)
            {
                double avg = doubles.Average();
                double sum = doubles.Sum(d => (d - avg) * (d - avg));
                ret = Math.Sqrt(sum / count);
            }

            return ret;
        }

        public static IEnumerable<Discontinuity> FindQueryGaps(this IEnumerable<MatchedWith> entries, double permittedGap, double fingerprintLength)
        {
            return entries
                .OrderBy(entry => entry.QueryMatchAt)
                .Select(m => Tuple.Create(m.QuerySequenceNumber, m.QueryMatchAt)).FindGaps(permittedGap, fingerprintLength);
        }

        public static IEnumerable<Discontinuity> FindTrackGaps(this IEnumerable<MatchedWith> entries, double trackLength, double permittedGap, double fingerprintLength)
        {
            var ordered = entries.OrderBy(m => m.TrackMatchAt)
                .Select(m => Tuple.Create(m.TrackSequenceNumber, m.TrackMatchAt))
                .ToList();

            (_, float startsAt) = ordered.First();
            if (startsAt > permittedGap)
            {
                yield return new Discontinuity(0, startsAt, true);
            }

            foreach (var discontinuity in ordered.FindGaps(permittedGap, fingerprintLength))
            {
                yield return discontinuity;
            }

            (_, float last) = ordered.Last();

            double endsAt = last + fingerprintLength;
            if (trackLength - endsAt > permittedGap)
            {
                yield return new Discontinuity(endsAt, trackLength, true);
            }
        }

        private static IEnumerable<Discontinuity> FindGaps(this IEnumerable<Tuple<uint, float>> entries, double permittedGap, double fingerprintLength)
        {
            var matches = entries.ToArray();
            for (int i = 1; i < matches.Length; ++i)
            {
                var startsAt = matches[i - 1].Item2;
                var endsAt = matches[i].Item2;
                var gap = (float)SubFingerprintsToSeconds.GapLengthToSeconds(endsAt, startsAt, fingerprintLength);
                var sequenceNumberIncrement = matches[i].Item1 - matches[i - 1].Item1;
                // ReSharper disable once RedundantCast, float is not sufficiently exact
                float start = (float)(endsAt - gap);
                float end = endsAt;
                if (!(end <= start) && gap > permittedGap && sequenceNumberIncrement > 1)
                {
                    yield return new Discontinuity(start, end, false);
                }
            }
        }
    }
}
