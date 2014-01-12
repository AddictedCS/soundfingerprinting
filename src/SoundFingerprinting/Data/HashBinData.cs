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

        [IgnoreBinding]
        public int HashTable { get; set; }

        public long HashBin { get; set; }

        [IgnoreBinding]
        public IModelReference SubFingerprintReference { get; set; }
    }
}
