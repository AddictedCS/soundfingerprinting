namespace Soundfingerprinting.Dao.Entities
{
    using System;

    using Soundfingerprinting.DbStorage.Entities;

    /// <summary>
    ///   Bin for Min-Hash + LSH schema
    /// </summary>
    [Serializable]
    public class HashBinMinHash : HashBin
    {
        public HashBinMinHash()
        {
        }

        public HashBinMinHash(int id, long hashBin, int hashTable, int trackId, int hashedFingerprint)
            : base(id, hashBin, hashTable, trackId)
        {
            FingerprintId = hashedFingerprint;
        }

        public int FingerprintId { get; set; }
    }
}