namespace SoundFingerprinting
{
    using System;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class ModelServiceInfo
    {
        public ModelServiceInfo(int tracksCount, int subFingerprintsCount, int[] hashCountsInTables)
        {
            TracksCount = tracksCount;
            SubFingerprintsCount = subFingerprintsCount;
            HashCountsInTables = hashCountsInTables;
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

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            var other = obj as ModelServiceInfo;

            if (other?.HashCountsInTables.Length != HashCountsInTables.Length)
            {
                return false;
            }

            for (int i = 0; i < HashCountsInTables.Length; ++i)
            {
                if (other.HashCountsInTables[i] != HashCountsInTables[i])
                {
                    return false;
                }
            }

            return TracksCount == other.TracksCount && SubFingerprintsCount == other.SubFingerprintsCount;
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
                $"{nameof(TracksCount)}: {TracksCount}, " +
                $"{nameof(SubFingerprintsCount)}: {SubFingerprintsCount}, " +
                $"{nameof(HashCountsInTables)}: [{string.Join(", ", HashCountsInTables)}]}}";
        }
    }
}
