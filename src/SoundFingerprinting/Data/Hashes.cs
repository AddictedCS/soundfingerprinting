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
        [ProtoMember(1)]
        private readonly List<HashedFingerprint> fingerprints;

        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds): this(fingerprints,durationInSeconds, DateTime.MinValue, string.Empty)
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

        public int Count => fingerprints.Count;

        public IEnumerator<HashedFingerprint> GetEnumerator()
        {
            return fingerprints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}