namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Query;

    public class LIS
    {
        public static IEnumerable<MatchedWith>[] Zip(IEnumerable<MatchedWith> matched, double maxGap = int.MaxValue)
        {
            // locking first dimension (TrackSequenceNumber)
            var matches = matched.OrderBy(x => x.TrackSequenceNumber).ToList();
            var maxIncreasingQuerySequence = matches.Select(_ => new MaxAt(1, _)).ToArray();

            int max = 1, maxIndex = 0;

            for (int life = 1; life < matches.Count; ++life)
            {
                for (int happiness = 0; happiness < life; ++happiness)
                {
                    bool queryIsIncreasing = matches[happiness].QuerySequenceNumber < matches[life].QuerySequenceNumber;
                    bool canExtendMax = maxIncreasingQuerySequence[happiness].Length + 1 > maxIncreasingQuerySequence[life].Length;
                    bool sameSequence = System.Math.Abs(matches[happiness].QueryMatchAt - matches[life].QueryMatchAt) < maxGap;
                    if (queryIsIncreasing && canExtendMax && sameSequence)
                    {
                        maxIncreasingQuerySequence[life] = new MaxAt(maxIncreasingQuerySequence[happiness].Length + 1, matches[life]);
                        if (maxIncreasingQuerySequence[life].Length >= max)
                        {
                            // it is important to have >= since in case if the query increase and the track is not 
                            // we need to move the sequence forward
                            max = maxIncreasingQuerySequence[life].Length;
                            maxIndex = life;
                        }
                    }
                }
            }

            var lis = new Stack<MaxAt>();
            for (int index = maxIndex; index >= 0; --index)
            {
                var current = maxIncreasingQuerySequence[index];
                if (current.Length == max) // found entry to insert into the final list
                {
                    var candidates = new List<MaxAt> {current};
                    for (int lookAheadIndex = index - 1;
                        lookAheadIndex >= 0 && current.Length == maxIncreasingQuerySequence[lookAheadIndex].Length;
                        --lookAheadIndex)
                    {
                        var lookAhead = maxIncreasingQuerySequence[lookAheadIndex];
                        // lis does not decrease meaning we need to pick best candidate by score
                        candidates.Add(lookAhead);
                    }

                    if (!lis.Any())
                    {
                        // select best candidate by score
                        // since no other elements are present, lets pick first
                        lis.Push(candidates.OrderByDescending(_ => _.MatchedWith.Score).First());
                    }
                    else
                    {
                        var last = lis.Peek();
                        foreach (var maxAt in candidates.OrderByDescending(_ => _.MatchedWith.Score))
                        {
                            // pick first best candidate from the same sequence
                            bool sameSequence = System.Math.Abs(last.MatchedWith.QueryMatchAt - maxAt.MatchedWith.QueryMatchAt) <= maxGap;
                            if (sameSequence)
                            {
                                lis.Push(maxAt);
                                break;
                            }
                        }
                    }

                    max--;
                }
            }

            var result = new List<MatchedWith>();
            while (lis.Any())
            {
                var current = lis.Pop();
                var lookAhead = lis.Any() ? lis.Peek() : new MaxAt(0, null);
                var candidates = new List<MaxAt> {current};
                while (current.MatchedWith.TrackSequenceNumber == lookAhead.MatchedWith?.TrackSequenceNumber)
                {
                    // lis peeked same track sequence entries, let's find best candidate of them all
                    candidates.Add(lis.Pop());
                    lookAhead = lis.Any() ? lis.Peek() : new MaxAt(0, null);
                }

                result.Add(candidates.OrderByDescending(_ => _.MatchedWith.Score).First().MatchedWith);
            }

            return new IEnumerable<MatchedWith>[] {result};
        }
    }
}