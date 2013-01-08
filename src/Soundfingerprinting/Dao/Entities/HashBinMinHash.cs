namespace Soundfingerprinting.DbStorage.Entities
{
    using System;

    /// <summary>
    ///   HashBin for Min-Hash + LSH schema
    /// </summary>
    [Serializable]
    public class HashBinMinHash : HashBin
    {
        public HashBinMinHash(int id, long hashBin, int hashTable, int trackId, int hashedFingerprint)
            : base(id, hashBin, hashTable, trackId)
        {
            HashedFingerprint = hashedFingerprint;
        }

        public int HashedFingerprint { get; set; }
    }
}