namespace SoundFingerprinting.DAO.Data
{
    using System;
    using System.Collections.Generic;

    using DAO;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class SubFingerprintData
    {
        public SubFingerprintData(int[] hashes, uint sequenceNumber, float sequenceAt, IEnumerable<string> clusters, IModelReference subFingerprintReference, IModelReference trackReference) 
            : this(hashes, sequenceNumber, sequenceAt, clusters, subFingerprintReference, trackReference, Array.Empty<byte>())
        {
        }
        
        public SubFingerprintData(int[] hashes, uint sequenceNumber, float sequenceAt, IEnumerable<string> clusters, IModelReference subFingerprintReference, IModelReference trackReference, byte[] originalPoint) : this()
        {
            Hashes = hashes;
            SubFingerprintReference = subFingerprintReference;
            TrackReference = trackReference;
            SequenceNumber = sequenceNumber;
            SequenceAt = sequenceAt;
            Clusters = clusters;
            OriginalPoint = originalPoint;
        }

        public SubFingerprintData()
        {
            Clusters = new List<string>();
        }

        [IgnoreBinding]
        [ProtoMember(1)]
        public int[] Hashes { get; }

        [ProtoMember(2)] 
        public uint SequenceNumber { get; }

        [ProtoMember(3)]
        public float SequenceAt { get; }

        [IgnoreBinding]
        [ProtoMember(4)]
        public IEnumerable<string> Clusters { get; }

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
