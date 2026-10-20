namespace SoundFingerprinting
{
    using System;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class ModelServiceInfo
    {
        public ModelServiceInfo(string id, int tracksCount, int subFingerprintsCount, int[] hashCountsInTables, long? sequence = null)
        {
            TracksCount = tracksCount;
            SubFingerprintsCount = subFingerprintsCount;
            HashCountsInTables = hashCountsInTables;
            Id = id;
            Sequence = sequence;
        }

        private ModelServiceInfo()
        {
            // left for proto-buf
        }

        [ProtoMember(1)]
        public int TracksCount { get; }

        [ProtoMember(2)]
        public int SubFingerprintsCount { get; }

        [ProtoMember(3)]
        public int[] HashCountsInTables { get; }

        [ProtoMember(4)]
        public string Id { get; }

        /// <summary>
        ///  Gets the sequence number of the last action the server applied, the snapshot for an insert that asks the server
        ///  to validate the client's identity decisions. Null from a server that does not report it.
        /// </summary>
        [ProtoMember(5)]
        public long? Sequence { get; }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            return obj is ModelServiceInfo other && other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TracksCount;
                hashCode = (hashCode * 397) ^ SubFingerprintsCount;
                hashCode = (hashCode * 397) ^ (HashCountsInTables != null ? HashCountsInTables.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}{{" +
                $"{nameof(Id)}: {Id}, " +   
                $"{nameof(TracksCount)}: {TracksCount}, " +
                $"{nameof(SubFingerprintsCount)}: {SubFingerprintsCount}, " +
                $"{nameof(HashCountsInTables)}: [{string.Join(", ", HashCountsInTables)}]}}";
        }
    }
}
