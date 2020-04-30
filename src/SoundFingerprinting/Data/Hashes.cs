namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;

    [Serializable]
    [ProtoContract(IgnoreListHandling = true, SkipConstructor = true)]
    public class Hashes : IEnumerable<HashedFingerprint>
    {
        private const double Accuracy = 1.48d;
        private const double FingerprintCount = 8192.0d / 5512;
        
        [ProtoMember(1)]
        private readonly List<HashedFingerprint> fingerprints;

        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds): this(fingerprints, durationInSeconds, DateTime.MinValue, string.Empty)
        {
        }
        
        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds, DateTime relativeTo, string origin)
        {
            this.fingerprints = fingerprints.ToList();
            DurationInSeconds = durationInSeconds;
            Origin = origin;
            RelativeTo = relativeTo;
        }

        [ProtoMember(2)]
        public double DurationInSeconds { get; }

        [ProtoMember(3)]
        public string Origin { get; }

        [ProtoMember(4)]
        public DateTime RelativeTo { get; }
        
        public static Hashes Empty => new Hashes(new List<HashedFingerprint>(), 0, DateTime.MinValue, string.Empty);
        
        public DateTime EndsAt => IsEmpty ? DateTime.MinValue : RelativeTo.Add(TimeSpan.FromSeconds(DurationInSeconds));

        public bool IsEmpty => !fingerprints.Any();
        
        public int Count => fingerprints.Count;

        public IEnumerator<HashedFingerprint> GetEnumerator()
        {
            return fingerprints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public bool MergeWith(Hashes with, out Hashes merged)
        {
            merged = null;

            if (IsEmpty)
            {
                merged = with;
                return true;
            }
            
            if (RelativeTo <= with.RelativeTo && EndsAt >= with.RelativeTo.Subtract(TimeSpan.FromSeconds(Accuracy)))
            {
                var result = Merge(fingerprints.OrderBy(h => h.SequenceNumber).ToList(), RelativeTo, with.fingerprints.OrderBy(h => h.SequenceNumber).ToList(), with.RelativeTo);
                var length = result.Last().StartsAt + FingerprintCount;
                var relativeTo = RelativeTo < with.RelativeTo ? RelativeTo : with.RelativeTo;
                merged = new Hashes(result, length, relativeTo, Origin);
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
                            stack.Push(merged);
                            if (merged.DurationInSeconds >= length)
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

        private static List<HashedFingerprint> Merge(IReadOnlyList<HashedFingerprint> first, DateTime firstStartsAt, IReadOnlyList<HashedFingerprint> second, DateTime secondStartsAt)
        {
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