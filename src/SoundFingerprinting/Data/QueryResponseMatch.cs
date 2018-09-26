namespace SoundFingerprinting.Data
{
    using System;

    using ProtoBuf;

    using SoundFingerprinting.DAO.Data;

    [Serializable]
    [ProtoContract]
    public class QueryResponseMatch
    {
        public QueryResponseMatch(SubFingerprintData subFingerprint, uint querySequenceNumber)
        {
            SubFingerprint = subFingerprint;
            QuerySequenceNumber = querySequenceNumber;
        }

        private QueryResponseMatch()
        {
            // left for protobuf
        }

        [ProtoMember(1)]
        public SubFingerprintData SubFingerprint { get; }

        [ProtoMember(2)]
        public uint QuerySequenceNumber { get; }
    }
}
