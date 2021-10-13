namespace SoundFingerprinting.Data
{
    using System;
    using ProtoBuf;

    /// <summary>
    ///  Hashed fingerprint class.
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class HashedFingerprint 
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="HashedFingerprint"/> class.
        /// </summary>
        /// <param name="hashBins">Array of integers describing the fingerprint.</param>
        /// <param name="sequenceNumber">Sequence number of the generated fingerprint.</param>
        /// <param name="startsAt">Time offset in seconds.</param>
        /// <param name="originalPoint">Original point from which the fingerprint was generated.</param>
        public HashedFingerprint(int[] hashBins, uint sequenceNumber, float startsAt, byte[] originalPoint)
        {
            HashBins = hashBins;
            SequenceNumber = sequenceNumber;
            StartsAt = startsAt;
            OriginalPoint = originalPoint;
        }

        private HashedFingerprint()
        {
            // Used only by .NET serializers
        }

        /// <summary>
        ///  Gets integers describing the fingerprint.
        /// </summary>
        [ProtoMember(1)]
        public int[] HashBins { get; }

        /// <summary>
        ///  Gets sequence number of the generated fingerprint.
        /// </summary>
        [ProtoMember(2)]
        public uint SequenceNumber { get; }

        /// <summary>
        ///  Gets time offset measured in seconds of the generated fingerprint.
        /// </summary>
        [ProtoMember(3)]
        public float StartsAt { get; }

        /// <summary>
        ///  Gets the original point from which the fingerprint was generated.
        /// </summary>
        [ProtoMember(5)] 
        public byte[] OriginalPoint { get; }
    }
}
