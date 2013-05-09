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

        public HashBin(int id, long hashBin, int hashTable)
        {
            Id = id;
            Bin = hashBin;
            HashTable = hashTable;
        }

        public int Id { get; set; }

        public long Bin { get; set; }

        public int HashTable { get; set; }
    }
}