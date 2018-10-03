namespace SoundFingerprinting
{
    public class ModelServiceInfo
    {
        public ModelServiceInfo(int tracksCount, int subFingerprintsCount, int[] hashCountsInTables)
        {
            TracksCount = tracksCount;
            SubFingerprintsCount = subFingerprintsCount;
            HashCountsInTables = hashCountsInTables;
        }

        public int TracksCount { get; }

        public int SubFingerprintsCount { get; }

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
    }
}
