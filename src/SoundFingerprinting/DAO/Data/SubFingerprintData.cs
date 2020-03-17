namespace SoundFingerprinting.DAO.Data
{
    using System;
    using DAO;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class SubFingerprintData
    {
        public SubFingerprintData(int[] hashes, uint sequenceNumber, float sequenceAt, IModelReference subFingerprintReference, IModelReference trackReference) 
            : this(hashes, sequenceNumber, sequenceAt, subFingerprintReference, trackReference, new byte[0])
        {
        }
        
        public SubFingerprintData(int[] hashes, uint sequenceNumber, float sequenceAt, IModelReference subFingerprintReference, IModelReference trackReference, byte[] originalPoint) : this()
        {
            Hashes = hashes;
            SubFingerprintReference = subFingerprintReference;
            TrackReference = trackReference;
            SequenceNumber = sequenceNumber;
            SequenceAt = sequenceAt;
            OriginalPoint = originalPoint;
        }

        private SubFingerprintData()
        {
        }

        [IgnoreBinding]
        [ProtoMember(1)]
        public int[] Hashes { get; }

        [ProtoMember(2)] 
        public uint SequenceNumber { get; }

        [ProtoMember(3)]
        public float SequenceAt { get; }

        public byte[] OriginalPoint { get; }

        [IgnoreBinding]
        [ProtoMember(5)]
        public IModelReference SubFingerprintReference { get; }

        [IgnoreBinding]
        [ProtoMember(6)]
        public IModelReference TrackReference { get; }
        
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
