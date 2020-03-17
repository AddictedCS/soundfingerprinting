namespace SoundFingerprinting.Data
{
    using System;
    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class HashedFingerprint 
    {
        public HashedFingerprint(int[] hashBins, uint sequenceNumber, float startsAt) : this(hashBins, sequenceNumber, startsAt, new byte[0])
        {
            
        }
        
        public HashedFingerprint(int[] hashBins, uint sequenceNumber, float startsAt, byte[] originalPoint)
        {
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            StartsAt = startsAt;
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

        [ProtoMember(5)] 
        public byte[] OriginalPoint { get; }
    }
}
