namespace SoundFingerprinting.Query
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using SoundFingerprinting.LCS;

    public static class Extensions
    {
        private const double PermittedGapZero = 1e-5;
        
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

        public static IEnumerable<Coverage> SplitTrackMatchedRegions(this IEnumerable<MatchedWith> entries,
            double queryLength, double trackLength, double fingerprintLength, double permittedGap)
        {
            var list = new List<Coverage>();
            var ordered =  entries.OrderBy(_ => _.TrackMatchAt).ToList();
            if (!ordered.Any())
            {
                return list;
            }

            var stack = new Stack<MatchedWith>();
            stack.Push(ordered.First());
            foreach (var matchedWith in ordered.Skip(1))
            {
                var prev = stack.Peek();
                if (SubFingerprintsToSeconds.GapLengthToSeconds(matchedWith.TrackMatchAt, prev.TrackMatchAt, fingerprintLength) > permittedGap)
                {
                    list.AddRange(GetMatchedWithsFromStack(stack, queryLength, trackLength, fingerprintLength, permittedGap));
                    stack = new Stack<MatchedWith>();
                }

                stack.Push(matchedWith);
            }

            list.AddRange(GetMatchedWithsFromStack(stack, queryLength, trackLength, fingerprintLength, permittedGap));
            return list;
        }

        private static IEnumerable<Coverage> GetMatchedWithsFromStack(Stack<MatchedWith> stack,
            double queryLength, double trackLength, double fingerprintLengthInSeconds, double permittedGap)
        {
            var matchedWiths = ((IEnumerable<MatchedWith>) stack.ToList()).Reverse().ToList();
            return matchedWiths.EstimateIncreasingCoverages(queryLength, trackLength, fingerprintLengthInSeconds, permittedGap);
        }

        public static IEnumerable<Gap> FindQueryGaps(this IEnumerable<MatchedWith> entries, double permittedGap, double fingerprintLength)
        {
            double sanitizedPermittedGap = permittedGap > 0 ? permittedGap : PermittedGapZero;
            return entries
                .OrderBy(entry => entry.QueryMatchAt)
                .Select(m => Tuple.Create(m.QuerySequenceNumber, m.QueryMatchAt)).FindGaps(sanitizedPermittedGap, fingerprintLength);
        }

        public static IEnumerable<Gap> FindTrackGaps(this IEnumerable<MatchedWith> entries, double trackLength, double permittedGap, double fingerprintLength)
        {
            double sanitizedPermittedGap = permittedGap > 0 ? permittedGap : PermittedGapZero;
            var ordered = entries.OrderBy(m => m.TrackMatchAt)
                .Select(m => Tuple.Create(m.TrackSequenceNumber, m.TrackMatchAt))
                .ToList();

            (_, float startsAt) = ordered.First();
            if (startsAt > sanitizedPermittedGap)
            {
                yield return new Gap(0, startsAt, true);
            }

            foreach (var gap in ordered.FindGaps(sanitizedPermittedGap, fingerprintLength))
            {
                yield return gap;
            }

            (_, float end) = ordered.Last();

            double endsAt =  end + fingerprintLength;
            if (trackLength - endsAt > sanitizedPermittedGap)
            {
                yield return new Gap(endsAt, trackLength, true);
            }
        }

        private static IEnumerable<Gap> FindGaps(this IEnumerable<Tuple<uint, float>> entries, double permittedGap, double fingerprintLength)
        {
            Tuple<uint, float>[] matches = entries.ToArray();
            for (int i = 1; i < matches.Length; ++i)
            {
                float startsAt = matches[i - 1].Item2;
                float endsAt = matches[i].Item2;
                float gap = (float)SubFingerprintsToSeconds.GapLengthToSeconds(endsAt, startsAt, fingerprintLength);
                bool sequenceNumberIncremented = matches[i].Item1 - matches[i - 1].Item1 > 1;
                float start = endsAt - gap;
                if (!(endsAt <= start) && gap > permittedGap && sequenceNumberIncremented)
                {
                    yield return new Gap(start, endsAt, false);
                }
            }
        }
    }
}
