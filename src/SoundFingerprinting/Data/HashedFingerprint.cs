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
            : this(hashBins, sequenceNumber, startsAt, clusters, Array.Empty<byte>())
        {
            
        }
        
        public HashedFingerprint(int[] hashBins, uint sequenceNumber, float startsAt, IEnumerable<string> clusters, byte[] originalPoint)
        {
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            StartsAt = startsAt;
            Clusters = clusters;
            OriginalPoint = originalPoint;
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
        
        [ProtoMember(5)] 
        public byte[] OriginalPoint { get; set; }
    }
}
