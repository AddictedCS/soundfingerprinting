namespace SoundFingerprinting.Data
{
    using System;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class QueryHash
    {
        public QueryHash(int[] hashes, uint sequenceNumber)
        {
            Hashes = hashes;
            SequenceNumber = sequenceNumber;
        }

        private QueryHash()
        {
            // left for protobuf
        }

        [ProtoMember(1)]
        public int[] Hashes { get; }

        [ProtoMember(2)]
        public uint SequenceNumber { get; }
    }
}
