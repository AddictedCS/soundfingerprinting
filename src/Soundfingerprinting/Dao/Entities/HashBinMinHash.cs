namespace Soundfingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public class HashBinMinHash : HashBin
    {
        public HashBinMinHash()
        {
            // no op
        }

        public HashBinMinHash(int id, long hashBin, int hashTable, long subFingerprintId)
            : base(id, hashBin, hashTable)
        {
            SubFingerprintId = subFingerprintId;
        }

        public long SubFingerprintId { get; set; }
    }
}