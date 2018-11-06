namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class HashedFingerprint 
    {
        public HashedFingerprint(int[] hashBins, uint sequenceNumber, float startsAt, IEnumerable<string> clusters)
        {
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            StartsAt = startsAt;
            Clusters = clusters;
        }

        private HashedFingerprint()
        {
            // Used only by protobuf
        }

        [ProtoMember(1)]
        public int[] HashBins { get; }

        [ProtoMember(2)]
        public uint SequenceNumber { get; }

        [ProtoMember(3)]
        public float StartsAt { get; }

        [ProtoMember(4)]
        public IEnumerable<string> Clusters { get; }
    }
}
