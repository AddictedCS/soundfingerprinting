namespace SoundFingerprinting.Data
{
    using System;

    using SoundFingerprinting.Dao;

    [Serializable]
    public class HashBinData
    {
        public HashBinData()
        {
            // no op    
        }

        public HashBinData(int hashTable, long hashBin)
        {
            HashTable = hashTable;
            HashBin = hashBin;
        }

        public HashBinData(int hashTable, long hashBin, ISubFingerprintReference subFingerprintReference)
            : this(hashTable, hashBin)
        {
            SubFingerprintReference = subFingerprintReference;
        }

        [IgnoreBinding]
        public int HashTable { get; set; }

        public long HashBin { get; set; }

        [IgnoreBinding]
        public ISubFingerprintReference SubFingerprintReference { get; set; }
    }
}
