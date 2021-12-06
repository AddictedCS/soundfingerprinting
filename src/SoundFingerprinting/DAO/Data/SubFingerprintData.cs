namespace SoundFingerprinting.DAO.Data
{
    using System;
    using ProtoBuf;
    using SoundFingerprinting.DAO;

    /// <summary>
    ///  Class storing hashes.
    /// </summary>
    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class SubFingerprintData : IEquatable<SubFingerprintData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubFingerprintData"/> class.
        /// </summary>
        /// <param name="hashes">Hashes.</param>
        /// <param name="sequenceNumber">Sequence number.</param>
        /// <param name="sequenceAt">Sequence at.</param>
        /// <param name="subFingerprintReference">Sub-fingerprint reference.</param>
        /// <param name="trackReference">Track reference.</param>
        public SubFingerprintData(int[] hashes, uint sequenceNumber, float sequenceAt, IModelReference subFingerprintReference, IModelReference trackReference) : this(hashes, sequenceNumber, sequenceAt, subFingerprintReference, trackReference, Array.Empty<byte>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubFingerprintData"/> class.
        /// </summary>
        /// <param name="hashes">Hashes.</param>
        /// <param name="sequenceNumber">Sequence number.</param>
        /// <param name="sequenceAt">Sequence at.</param>
        /// <param name="subFingerprintReference">Sub-fingerprint reference.</param>
        /// <param name="trackReference">Track reference.</param>
        /// <param name="originalPoint">Original point.</param>
        public SubFingerprintData(int[] hashes, uint sequenceNumber, float sequenceAt, IModelReference subFingerprintReference, IModelReference trackReference, byte[] originalPoint)
        {
            Hashes = hashes;
            SubFingerprintReference = subFingerprintReference;
            TrackReference = trackReference;
            SequenceNumber = sequenceNumber;
            SequenceAt = sequenceAt;
            OriginalPoint = originalPoint;
        }

        /// <summary>
        ///  Gets hashes stored in an array where each value corresponds to a hash-bin in the hash-table.
        /// </summary>
        [IgnoreBinding]
        [ProtoMember(1)]
        public int[] Hashes { get; }

        /// <summary>
        ///  Gets sequence number.
        /// </summary>
        [ProtoMember(2)] 
        public uint SequenceNumber { get; }

        /// <summary>
        ///  Gets sequence at.
        /// </summary>
        [ProtoMember(3)]
        public float SequenceAt { get; }

        /// <summary>
        ///  Gets original point (empty if not set).
        /// </summary>
        public byte[] OriginalPoint { get; }

        /// <summary>
        ///  Gets sub-fingerprint reference.
        /// </summary>
        [IgnoreBinding]
        [ProtoMember(5)]
        public IModelReference SubFingerprintReference { get; }

        /// <summary>
        ///  Gets track reference.
        /// </summary>
        [IgnoreBinding]
        [ProtoMember(6)]
        public IModelReference TrackReference { get; }
        
        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return Equals(obj as SubFingerprintData);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return SubFingerprintReference.GetHashCode();
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(SubFingerprintData? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return SubFingerprintReference.Equals(other.SubFingerprintReference);
        }
    }
}
