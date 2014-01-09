namespace SoundFingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    internal class HashBinMinHash
    {
        public HashBinMinHash()
        {
            // no op
        }

        public HashBinMinHash(long hashBin, int hashTable, long subFingerprintId)
            : this()
        {
            HashBin = hashBin;
            HashTable = hashTable;
            SubFingerprintId = subFingerprintId;
        }

        public long HashBin { get; set; }

        public int HashTable { get; set; }

        public long SubFingerprintId { get; set; }
    }
}