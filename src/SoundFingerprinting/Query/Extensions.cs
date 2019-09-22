using System.Collections.Generic;
namespace SoundFingerprinting.Query
{
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

        public static IEnumerable<Discontinuity> FindGaps(this IEnumerable<float> entries, double permittedGap)
        {
            var floats = entries.OrderBy(entry => entry).ToArray();
            for (int i = 1; i < floats.Length; ++i)
            {
                if (floats[i] - floats[i - 1] > permittedGap)
                {
                    yield return new Discontinuity(floats[i - 1], floats[i]);
                }
            }
        }
    }
}
