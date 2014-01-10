namespace SoundFingerprinting.Data
{
    public class HashBinData
    {
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

        public int HashTable { get; private set; }

        public long HashBin { get; private set; }

        public ISubFingerprintReference SubFingerprintReference { get; private set; }
    }
}
