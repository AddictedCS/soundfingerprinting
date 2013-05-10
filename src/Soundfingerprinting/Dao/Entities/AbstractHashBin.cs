namespace Soundfingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public abstract class AbstractHashBin
    {
        protected AbstractHashBin()
        {
            Id = int.MinValue;
        }

        protected AbstractHashBin(int id, long hashBin, int hashTable)
        {
            Id = id;
            HashBin = hashBin;
            HashTable = hashTable;
        }

        public int Id { get; set; }

        public long HashBin { get; set; }

        public int HashTable { get; set; }
    }
}