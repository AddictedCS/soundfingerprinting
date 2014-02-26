namespace SoundFingerprinting.Data
{
    using System;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.SQL;

    [Serializable]
    public class HashBinData
    {
        [IgnoreBinding]
        public int HashTable { get; set; }

        public long HashBin { get; set; }

        [IgnoreBinding]
        public IModelReference SubFingerprintReference { get; set; }
    }
}
