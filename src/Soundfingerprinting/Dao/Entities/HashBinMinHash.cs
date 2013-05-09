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

        public HashBinMinHash(int id, long hashBin, int hashTable, int fingerprintId)
            : base(id, hashBin, hashTable)
        {
            FingerprintId = fingerprintId;
        }

        public int FingerprintId { get; set; }
    }
}