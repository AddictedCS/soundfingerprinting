namespace SoundFingerprinting
{
    using System;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class ModelServiceInfo
    {
        public ModelServiceInfo(string id, int tracksCount, int subFingerprintsCount, int[] hashCountsInTables)
        {
            TracksCount = tracksCount;
            SubFingerprintsCount = subFingerprintsCount;
            HashCountsInTables = hashCountsInTables;
            Id = id;
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
