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

        public static IEnumerable<Discontinuity> FindGaps(this IEnumerable<Tuple<uint, float>> entries, double permittedGap, double fingerprintLength)
        {
            var matches = entries.OrderBy(entry => entry.Item2).ToArray();
            for (int i = 1; i < matches.Length; ++i)
            {
                var startsAt = matches[i - 1].Item2;
                var endsAt = matches[i].Item2;
                var gap = (float)SubFingerprintsToSeconds.GapLengthToSeconds(endsAt, startsAt, fingerprintLength);
                var sequenceNumberIncrement = matches[i].Item1 - matches[i - 1].Item1;

                if (gap > permittedGap && sequenceNumberIncrement > 1)
                {
                    yield return new Discontinuity(endsAt - gap, endsAt);
                }
            }
        }
    }
}
