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
                ret = System.Math.Sqrt(sum / count);
            }

            return ret;
        }

        public static IEnumerable<Discontinuity> FindGaps(this IEnumerable<Tuple<uint, float>> entries, double permittedGap, double fingerprintLength)
        {
            var floats = entries.OrderBy(entry => entry.Item2).ToArray();
            for (int i = 1; i < floats.Length; ++i)
            {
                if (floats[i].Item2 - (floats[i - 1].Item2 + fingerprintLength) > permittedGap && floats[i].Item1 - floats[i - 1].Item1 > 1)
                {
                    yield return new Discontinuity(floats[i - 1].Item2 + (float)fingerprintLength, floats[i].Item2);
                }
            }
        }
    }
}
