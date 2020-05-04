namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;

    [Serializable]
    [ProtoContract(IgnoreListHandling = true)]
    public class Hashes : IEnumerable<HashedFingerprint>
    {
        private const double MergeAccuracy = 1.48d;

        [ProtoMember(1)]
        private readonly List<HashedFingerprint> fingerprints;

        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds):
            this(fingerprints,
                durationInSeconds,
                DateTime.Now,
                Enumerable.Empty<string>(),
                string.Empty)
        {
        }

        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds, DateTime relativeTo):
            this(fingerprints,
                durationInSeconds,
                relativeTo,
                Enumerable.Empty<string>(),
                string.Empty)
        {
        }

        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds, DateTime relativeTo, IEnumerable<string> origins):
            this(fingerprints,
                durationInSeconds,
                relativeTo,
                origins,
                string.Empty)
        {
        }

        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds, DateTime relativeTo, IEnumerable<string> origins, string streamId)
        {
            this.fingerprints = fingerprints.ToList();
            if (this.fingerprints.Any() && durationInSeconds <= 0)
            {
                throw new ArgumentException(nameof(durationInSeconds));
            }
            
            DurationInSeconds = durationInSeconds;
            RelativeTo = relativeTo;
            Origins = origins;
            StreamId = streamId;
        }

        private Hashes()
        {
            // left for proto-buf
        }

        [ProtoMember(2)]
        public double DurationInSeconds { get; }

        [ProtoMember(3)] 
        public string StreamId { get; }

        [ProtoMember(4)]
        public DateTime RelativeTo { get; }

        [ProtoMember(5)]
        public IEnumerable<string> Origins { get; }

        public DateTime EndsAt
        {
            get
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Hashes are empty, EndsAt is undefined");
                }

                if (RelativeTo.Equals(DateTime.MinValue))
                {
                    throw new InvalidOperationException("RelativeTo was not supplied as a parameter to find EndsAt");
                }
                
                return RelativeTo.Add(TimeSpan.FromSeconds(DurationInSeconds));
            }
        }

        public bool IsEmpty => !fingerprints.Any();
        
        public int Count => fingerprints.Count;

        public static Hashes Empty => new Hashes(new List<HashedFingerprint>(), 0, DateTime.MinValue, new List<string>(), string.Empty);
        
        public Hashes WithStreamId(string streamId)
        {
            return new Hashes(fingerprints, DurationInSeconds, RelativeTo, Origins, streamId);
        }

        public Hashes WithNewRelativeTo(DateTime relativeTo)
        {
            return new Hashes(fingerprints, DurationInSeconds, relativeTo, Origins, StreamId);
        }

        public IEnumerator<HashedFingerprint> GetEnumerator()
        {
            return fingerprints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public bool MergeWith(Hashes with, out Hashes? merged)
        {
            merged = null;

            if (IsEmpty)
            {
                merged = with;
                return true;
            }

            if (with.IsEmpty)
            {
                merged = this;
                return true;
            }

            string streamId = (StreamId, with.StreamId) switch
            {
                ("", "") => "",
                (string left, "") => left,
                ("", string right) => right,
                (string left, string right) when (left.Equals(right)) => left,
                _ => throw new NotSupportedException($"Can't merge two hash sequences that come with different streams {StreamId}, {with.StreamId}")
            };

            if (RelativeTo <= with.RelativeTo && EndsAt >= with.RelativeTo.Subtract(TimeSpan.FromSeconds(MergeAccuracy)))
            {
                var result = Merge(this, with);
                uint count = result.Last().SequenceNumber - result.First().SequenceNumber;
                float length = result.Last().StartsAt - result.First().StartsAt;
                float lengthOfOneHash = length / count;
                float fullLength = result.Last().StartsAt + lengthOfOneHash;
                var relativeTo = RelativeTo < with.RelativeTo ? RelativeTo : with.RelativeTo;
                merged = new Hashes(result, fullLength, relativeTo, new HashSet<string>(Origins.Concat(with.Origins)), streamId);
                return true;
            }

            return false;
        }

        public static IEnumerable<Hashes> Aggregate(IEnumerable<Hashes> hashes, double length)
        {
            return hashes
                .OrderBy(entry => entry.RelativeTo)
                .Aggregate(new Stack<Hashes>(new[] {Empty}), (stack, next) =>
                    {
                        var completed = stack.Pop();
                        if (completed.MergeWith(next, out var merged))
                        {
                            stack.Push(merged!);
                            if (merged!.DurationInSeconds >= length)
                            {
                                stack.Push(Empty);
                            }
                        }
                        else
                        {
                            stack.Push(completed);
                            stack.Push(next);
                        }

                        return stack;
                    },
                    stack =>
                    {
                        var result = new List<Hashes>();
                        while (stack.Any())
                        {
                            result.Add(stack.Pop());
                        }

                        return result;
                    });
        }

        private static List<HashedFingerprint> Merge(Hashes left, Hashes right)
        {
            var first = left.OrderBy(_ => _.SequenceNumber).ToList();
            var firstStartsAt = left.RelativeTo;
            var second = right.OrderBy(_ => _.SequenceNumber).ToList();
            var secondStartsAt = right.RelativeTo;
                
            var result = new List<HashedFingerprint>();
            int i = 0, j = 0;
            var diff = secondStartsAt.Subtract(firstStartsAt);
            
            for (int k = 0; k < first.Count + second.Count; ++k)
            {
                if (i == first.Count)
                {
                    var startAt = diff.TotalSeconds + second[j].StartsAt;
                    result.Add(new HashedFingerprint(second[j].HashBins, (uint)k, (float)startAt));
                    ++j;
                }
                else if (j == second.Count)
                {
                    result.Add(new HashedFingerprint(first[i].HashBins, (uint)k, first[i].StartsAt));
                    ++i;
                }
                else if (firstStartsAt.AddSeconds(first[i].StartsAt) <= secondStartsAt.AddSeconds(second[j].StartsAt))
                {
                    result.Add(new HashedFingerprint(first[i].HashBins, (uint)k, first[i].StartsAt));
                    ++i;
                }
                else
                {
                    var startAt = diff.TotalSeconds + second[j].StartsAt;
                    result.Add(new HashedFingerprint(second[j].HashBins, (uint)k, (float)startAt));
                    ++j;
                }
            }

            return result;
        }
    }
}