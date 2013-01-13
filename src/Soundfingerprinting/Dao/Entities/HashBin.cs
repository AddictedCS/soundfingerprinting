namespace Soundfingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public class HashBin
    {
        public HashBin()
        {
            Id = int.MinValue;
        }

        public HashBin(int id, long hashBin, int hashTable, int trackId)
        {
            Id = id;
            Bin = hashBin;
            HashTable = hashTable;
            TrackId = trackId;
        }

        public int Id { get; set; }

        public long Bin { get; set; }

        public int HashTable { get; set; }

        public int TrackId { get; set; }
    }
}