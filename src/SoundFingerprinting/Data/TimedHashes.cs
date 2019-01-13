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
        private const double FingerprintLength = 8192.0d / 5512;
        
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

        private DateTime EndsAt => StartsAt.Add(TimeSpan.FromSeconds(hashedFingerprints.Last().StartsAt + FingerprintLength));

        public bool MergeWith(TimedHashes with, out TimedHashes merged)
        {
            merged = null;
            
            if (StartsAt <= with.StartsAt && EndsAt <= with.StartsAt.Subtract(TimeSpan.FromSeconds(Accuracy)))
            {
                HashedFingerprint[] first = hashedFingerprints.ToArray();
                HashedFingerprint[] seconds = with.hashedFingerprints.ToArray();

                var result = new List<HashedFingerprint>();
                int i = 0, j = 0;
                for (int k = 0; k < first.Length + seconds.Length; ++k)
                {
                    if (i == first.Length)
                    {
                        result.Add(seconds[j++]);
                    }
                    else if (j == seconds.Length)
                    {
                        result.Add(first[i++]);
                    }
                    else if (first[i].StartsAt <= seconds[j].StartsAt)
                    {
                        result.Add(first[i++]);
                    }
                    else
                    {
                        result.Add(seconds[j++]);
                    }
                }
                
                merged = new TimedHashes(result, StartsAt);
                return true;
            }
            
            return false;
        }
    }
}