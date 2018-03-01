namespace SoundFingerprinting.DAO.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DAO;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class SubFingerprintData
    {
        public SubFingerprintData(int[] hashes, uint sequenceNumber, float sequenceAt, IModelReference subFingerprintReference, IModelReference trackReference) : this()
        {
            Hashes = hashes;
            SubFingerprintReference = subFingerprintReference;
            TrackReference = trackReference;
            SequenceNumber = sequenceNumber;
            SequenceAt = sequenceAt;
        }

        public SubFingerprintData()
        {
            Clusters = new List<string>();
        }

        [IgnoreBinding]
        [ProtoMember(1)]
        public int[] Hashes { get; internal set; }

        [ProtoMember(2)] 
        public uint SequenceNumber { get; internal set; }

        [ProtoMember(3)]
        public float SequenceAt { get; internal set; }

        [IgnoreBinding]
        [ProtoMember(4)]
        public IEnumerable<string> Clusters { get; internal set; }

        [IgnoreBinding]
        [ProtoMember(5)]
        public IModelReference SubFingerprintReference { get; internal set; }

        [IgnoreBinding]
        [ProtoMember(6)]
        public IModelReference TrackReference { get; internal set; }

        public override bool Equals(object obj)
        {
            if (!(obj is SubFingerprintData))
            {
                return false;
            }

            return ((SubFingerprintData)obj).SubFingerprintReference.Equals(SubFingerprintReference);
        }

        public override int GetHashCode()
        {
            return SubFingerprintReference.GetHashCode();
        }
    }
}
