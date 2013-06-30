namespace SoundFingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public class HashBinNeuralHasher : AbstractHashBin
    {
        public HashBinNeuralHasher(long hashBin, int hashTable, int trackId)
            : base(hashBin, hashTable)
        {
            TrackId = trackId;
        }

        public int TrackId { get; set; }
    }
}