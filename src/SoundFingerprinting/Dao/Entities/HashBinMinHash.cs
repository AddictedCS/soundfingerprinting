namespace SoundFingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public class HashBinMinHash : AbstractHashBin
    {
        public HashBinMinHash()
        {
            // no op
        }

        public HashBinMinHash(long hashBin, int hashTable, long subFingerprintId)
            : base(hashBin, hashTable)
        {
            SubFingerprintId = subFingerprintId;
        }

        public long SubFingerprintId { get; set; }
    }
}