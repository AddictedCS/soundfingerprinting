namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;

    [ProtoContract]
    public class TimedHashes
    {
        private const double Accuracy = 1.48d;
        private const double FingerprintCount = 8192.0d / 5512;
        
        [ProtoMember(1)]
        private readonly List<HashedFingerprint> hashedFingerprints = new List<HashedFingerprint>();
        
        public TimedHashes(List<HashedFingerprint> hashedFingerprints, DateTime startsAt)
        {
            this.hashedFingerprints = hashedFingerprints;
            StartsAt = startsAt;
        }

        private TimedHashes()
        {
            // left for proto-buf
        }

        public IList<HashedFingerprint> HashedFingerprints => hashedFingerprints;

        [ProtoMember(2)]
        public DateTime StartsAt { get; }

        private DateTime EndsAt => StartsAt.Add(TimeSpan.FromSeconds(hashedFingerprints.Last().StartsAt + FingerprintCount));

        public bool MergeWith(TimedHashes with, out TimedHashes merged)
        {
            merged = null;
            
            if (StartsAt <= with.StartsAt && EndsAt >= with.StartsAt.Subtract(TimeSpan.FromSeconds(Accuracy)))
            {
                var result = Merge(hashedFingerprints.OrderBy(h => h.SequenceNumber).ToList(), StartsAt, with.hashedFingerprints.OrderBy(h => h.SequenceNumber).ToList(), with.StartsAt);
                merged = new TimedHashes(result, StartsAt);
                return true;
            }
            
            return false;
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
                    var startAt = diff.TotalSeconds + j * FingerprintCount;
                    result.Add(new HashedFingerprint(second[j].HashBins, (uint)k, (float)startAt, second[j].Clusters));
                    ++j;
                }
                else if (j == second.Count)
                {
                    var startsAt = i * FingerprintCount;
                    result.Add(new HashedFingerprint(first[i].HashBins, (uint)k, (float)startsAt, first[i].Clusters));
                    ++i;
                }
                else if (firstStartsAt.Add(TimeSpan.FromSeconds(first[i].StartsAt)) <= secondStartsAt.Add(TimeSpan.FromSeconds(second[j].StartsAt)))
                {
                    var startsAt = i * FingerprintCount;
                    result.Add(new HashedFingerprint(first[i].HashBins, (uint)k, (float)startsAt, first[i].Clusters));
                    ++i;
                }
                else
                {
                    var startAt = diff.TotalSeconds + j * FingerprintCount;
                    result.Add(new HashedFingerprint(second[j].HashBins, (uint)k, (float)startAt, second[j].Clusters));
                    ++j;
                }
            }

            return result;
        }
    }
}