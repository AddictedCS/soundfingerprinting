namespace SoundFingerprinting.Dao.Entities
{
    using System;

    [Serializable]
    public class HashBinNeuralHasher : AbstractHashBin
    {
        public HashBinNeuralHasher(int id, long hashBin, int hashTable, int trackId) : base(id, hashBin, hashTable)
        {
            TrackId = trackId;
        }

        public int TrackId { get; set; }
    }
}