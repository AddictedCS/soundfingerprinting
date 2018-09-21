namespace SoundFingerprinting.Data
{
    using System;

    using ProtoBuf;

    using SoundFingerprinting.DAO.Data;

    [Serializable]
    [ProtoContract]
    public class SubFingerprintInfo
    {
        public SubFingerprintInfo(SubFingerprintData subFingerprint, int querySequenceNumber)
        {
            SubFingerprint = subFingerprint;
            QuerySequenceNumber = querySequenceNumber;
        }

        [ProtoMember(1)]
        public SubFingerprintData SubFingerprint { get; }

        [ProtoMember(2)]
        public int QuerySequenceNumber { get; }
    }
}
