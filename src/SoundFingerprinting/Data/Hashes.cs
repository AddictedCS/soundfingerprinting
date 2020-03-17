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
        [ProtoMember(1)]
        private readonly List<HashedFingerprint> fingerprints;

        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds)
        {
            this.fingerprints = fingerprints.ToList();
            DurationInSeconds = durationInSeconds;
        }

        private Hashes()
        {
            // left for proto-buf
        }
        
        [ProtoMember(2)]
        public double DurationInSeconds { get; }

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